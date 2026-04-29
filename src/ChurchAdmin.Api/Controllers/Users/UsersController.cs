using ChurchAdmin.Api.Common;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Users;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly PasswordHasher _passwordHasher;
    private readonly EmailService _emailService;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly IValidator<UpdateUserRoleRequest> _roleValidator;

    public UsersController(
        IChurchAdminDbContext db,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        PasswordHasher passwordHasher,
        EmailService emailService,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator,
        IValidator<UpdateUserRoleRequest> roleValidator)
    {
        _db = db;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _roleValidator = roleValidator;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        List<User> users = await _db.Users
            .OrderBy(x => x.Email)
            .ToListAsync();

        return users.Select(UserResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        return UserResponse.FromEntity(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        string email = request.Email.Trim().ToLowerInvariant();

        bool emailExists = await _db.Users.AnyAsync(x => x.Email == email);

        if (emailExists)
        {
            return Conflict("A user with this email already exists.");
        }

        string inviteToken = _passwordHasher.GenerateInviteToken();
        string inviteTokenHash = _passwordHasher.HashToken(inviteToken);

        User user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            Role = request.Role,
            IsActive = request.IsActive,
            PasswordHash = null,
            InviteTokenHash = inviteTokenHash,
            InviteTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            InviteAcceptedAt = null,
            ExternalProvider = "Local"
        };

        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        UserResponse response = UserResponse.FromEntity(user);
        response.InviteLink = $"http://localhost:4173/set-password?token={inviteToken}";

        await _emailService.SendInviteEmailAsync(
            user.Email,
            user.DisplayName,
            response.InviteLink);

        await _auditService.LogAsync(
            "User",
            user.Id,
            AuditAction.Created,
            before: null,
            after: response,
            reason: "User created by admin");

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        User? user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        UserResponse before = UserResponse.FromEntity(user);

        user.DisplayName = request.DisplayName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "User",
            user.Id,
            AuditAction.Updated,
            before: before,
            after: UserResponse.FromEntity(user),
            reason: "User updated by admin");

        return NoContent();
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        UpdateUserRoleRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _roleValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        User? user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        UserResponse before = UserResponse.FromEntity(user);

        user.Role = request.Role;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "User",
            user.Id,
            AuditAction.Updated,
            before: before,
            after: UserResponse.FromEntity(user),
            reason: "User role updated");

        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        UserResponse before = UserResponse.FromEntity(user);

        user.IsActive = true;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "User",
            user.Id,
            AuditAction.Activated,
            before: before,
            after: UserResponse.FromEntity(user),
            reason: "User activated");

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        if (_currentUserService.UserId == user.Id.ToString())
        {
            return BadRequest("You cannot deactivate your own account.");
        }

        UserResponse before = UserResponse.FromEntity(user);

        user.IsActive = false;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "User",
            user.Id,
            AuditAction.Deactivated,
            before: before,
            after: UserResponse.FromEntity(user),
            reason: "User deactivated");

        return NoContent();
    }
}