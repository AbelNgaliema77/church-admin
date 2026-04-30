using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Users;

public sealed class UpdateUserRequest
{
    public string DisplayName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.Role)
            .IsInEnum();
    }
}