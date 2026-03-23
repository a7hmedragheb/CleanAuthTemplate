using Google.Apis.Auth;

namespace Template.Api.Services;
public interface IGoogleAuthService
{
	Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken);
}
