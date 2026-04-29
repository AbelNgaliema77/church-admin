using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChurchAdmin.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChurchAdmin.Api.Common;

public sealed class JwtTokenService
{
    private readonly AuthSettings _authSettings;

    public JwtTokenService(IOptions<AuthSettings> authSettings)
    {
        _authSettings = authSettings.Value;
    }

    public string CreateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_authSettings.JwtKey))
        {
            throw new InvalidOperationException("JWT key is not configured.");
        }

        SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_authSettings.JwtKey));

        SigningCredentials credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        ];

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _authSettings.Issuer,
            audience: _authSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_authSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}