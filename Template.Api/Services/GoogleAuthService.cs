using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Template.Api.Services;
public class GoogleAuthService : IGoogleAuthService
{
	private readonly GoogleSettings _googleSettings;

	public GoogleAuthService(IOptions<GoogleSettings> googleSettings)
	{
		_googleSettings = googleSettings.Value;
	}

	public async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken)
	{
		try
		{
			var settings = new GoogleJsonWebSignature.ValidationSettings
			{
				Audience = [_googleSettings.ClientId]
			};

			return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
		}
		catch
		{
			return null; 
		}
	}
}