namespace CrewFlow.Application.Auth;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record LoginRequest(string Email, string Password);

public record ExternalLoginRequest(string Provider, string IdToken);

public record RefreshRequest(string RefreshToken);

public record AuthTokensResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken);

public record CurrentUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    Guid? MemberId);
