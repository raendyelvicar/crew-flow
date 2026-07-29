namespace CrewFlow.Application.Common.Interfaces;

public record ExternalUserInfo(string Provider, string ProviderUserId, string Email, string? FirstName, string? LastName);

public interface IExternalAuthVerifier
{
    Task<ExternalUserInfo> VerifyGoogleIdTokenAsync(string idToken, CancellationToken ct = default);
}
