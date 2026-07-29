using CrewFlow.Domain.Identity;

namespace CrewFlow.Application.Common.Interfaces;

public record AccessTokenResult(string AccessToken, DateTime ExpiresAtUtc);

public interface ITokenService
{
    Task<AccessTokenResult> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles);

    // Returns the plaintext refresh token (given to the client); only its hash is persisted.
    (string PlainTextToken, RefreshToken Entity) GenerateRefreshToken(Guid userId);

    string HashToken(string plainTextToken);
}
