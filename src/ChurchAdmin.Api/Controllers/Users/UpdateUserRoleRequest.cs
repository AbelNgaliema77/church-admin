using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Users;

public sealed class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

public sealed class UpdateUserRoleRequestValidator
    : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum();
    }
}