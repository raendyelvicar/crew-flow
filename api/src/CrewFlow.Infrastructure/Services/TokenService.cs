using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Domain.Identity;
using CrewFlow.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrewFlow.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<AccessTokenResult> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("first_name", user.FirstName),
            new("last_name", user.LastName),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(new AccessTokenResult(accessToken, expiresAt));
    }

    public (string PlainTextToken, RefreshToken Entity) GenerateRefreshToken(Guid userId)
    {
        var plainTextToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(plainTextToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshTokenDays),
        };

        return (plainTextToken, entity);
    }

    public string HashToken(string plainTextToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainTextToken));
        return Convert.ToHexString(bytes);
    }
}
