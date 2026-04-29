using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Auth;

public sealed class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}

public sealed class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty();
    }
}