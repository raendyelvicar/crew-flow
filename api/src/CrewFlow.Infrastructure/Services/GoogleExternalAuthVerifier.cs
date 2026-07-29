using CrewFlow.Application.Common.Interfaces;
using CrewFlow.Infrastructure.Options;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CrewFlow.Infrastructure.Services;

public class GoogleExternalAuthVerifier : IExternalAuthVerifier
{
    private readonly GoogleAuthOptions _options;

    public GoogleExternalAuthVerifier(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<ExternalUserInfo> VerifyGoogleIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [_options.ClientId],
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

        return new ExternalUserInfo("Google", payload.Subject, payload.Email, payload.GivenName, payload.FamilyName);
    }
}
