using ChurchAdmin.Api.Common;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using FluentValidation;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChurchAdmin.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AuthSettings _authSettings;
    private readonly PasswordHasher _passwordHasher;
    private readonly IValidator<GoogleLoginRequest> _googleValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<SetPasswordRequest> _setPasswordValidator;

    public AuthController(
        IChurchAdminDbContext db,
        JwtTokenService jwtTokenService,
        IOptions<AuthSettings> authSettings,
        PasswordHasher passwordHasher,
        IValidator<GoogleLoginRequest> googleValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<SetPasswordRequest> setPasswordValidator)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _authSettings = authSettings.Value;
        _passwordHasher = passwordHasher;
        _googleValidator = googleValidator;
        _loginValidator = loginValidator;
        _setPasswordValidator = setPasswordValidator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _loginValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        string email = request.Email.Trim().ToLowerInvariant();

            User? user = await _db.Users
         .Include(x => x.Church)
         .FirstOrDefaultAsync(x =>
             x.Email == email &&
             x.Church.Slug == request.ChurchSlug);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Unauthorized("User account is inactive.");
        }

        bool passwordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        string token = _jwtTokenService.CreateToken(user);

        return AuthResponse.FromUser(user, token);
    }

    [HttpPost("set-password")]
    public async Task<ActionResult<AuthResponse>> SetPassword(SetPasswordRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _setPasswordValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        string tokenHash = _passwordHasher.HashToken(request.Token);

        User? user = await _db.Users.FirstOrDefaultAsync(
            x => x.InviteTokenHash == tokenHash);

        if (user is null)
        {
            return Unauthorized("Invalid invite link.");
        }

        if (user.InviteTokenExpiresAt is null ||
            user.InviteTokenExpiresAt < DateTimeOffset.UtcNow)
        {
            return Unauthorized("Invite link has expired.");
        }

        if (!user.IsActive)
        {
            return Unauthorized("User account is inactive.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        user.InviteTokenHash = null;
        user.InviteTokenExpiresAt = null;
        user.InviteAcceptedAt = DateTimeOffset.UtcNow;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.ExternalProvider = "Local";

        await _db.SaveChangesAsync();

        string jwt = _jwtTokenService.CreateToken(user);

        return AuthResponse.FromUser(user, jwt);
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> GoogleLogin(GoogleLoginRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _googleValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        if (string.IsNullOrWhiteSpace(_authSettings.GoogleClientId))
        {
            return BadRequest("Google client ID is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_authSettings.GoogleClientId]
                });
        }
        catch
        {
            return Unauthorized("Invalid Google token.");
        }

        User user = await GetOrCreateExternalUserAsync(
            email: payload.Email,
            displayName: payload.Name ?? payload.Email,
            provider: "Google",
            providerUserId: payload.Subject);

        if (!user.IsActive)
        {
            return Unauthorized("User account is inactive.");
        }

        string token = _jwtTokenService.CreateToken(user);

        return AuthResponse.FromUser(user, token);
    }

    private async Task<User> GetOrCreateExternalUserAsync(
        string email,
        string displayName,
        string provider,
        string providerUserId)
    {
        string cleanEmail = email.Trim().ToLowerInvariant();

        User? existingUser = await _db.Users.FirstOrDefaultAsync(
            x => x.Email == cleanEmail);

        if (existingUser is not null)
        {
            existingUser.DisplayName = displayName;
            existingUser.ExternalProvider = provider;
            existingUser.ExternalProviderUserId = providerUserId;
            existingUser.LastLoginAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            return existingUser;
        }

        User newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = cleanEmail,
            DisplayName = displayName,
            ExternalProvider = provider,
            ExternalProviderUserId = providerUserId,
            Role = UserRole.Pending,
            IsActive = true,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(newUser);

        await _db.SaveChangesAsync();

        return newUser;
    }
}