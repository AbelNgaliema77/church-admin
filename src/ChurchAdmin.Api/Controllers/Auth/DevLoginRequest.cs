using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Auth;

public sealed class DevLoginRequest
{
    public string Email { get; set; } = string.Empty;
}

public sealed class DevLoginRequestValidator : AbstractValidator<DevLoginRequest>
{
    public DevLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}