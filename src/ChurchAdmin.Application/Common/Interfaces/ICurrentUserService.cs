namespace ChurchAdmin.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string Email { get; }
    Guid? ChurchId { get; }
}
