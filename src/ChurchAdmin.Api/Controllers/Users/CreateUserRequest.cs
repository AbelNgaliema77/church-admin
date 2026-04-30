using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Users;

public sealed class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Pending;

    public bool IsActive { get; set; } = true;
}

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.Role)
            .IsInEnum();
    }
}