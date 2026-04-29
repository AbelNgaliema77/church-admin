namespace ChurchAdmin.Api.Common;

public sealed class AuthSettings
{
    public string JwtKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 120;

    public string GoogleClientId { get; set; } = string.Empty;
}