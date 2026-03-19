using Template.Api.Contracts.Auth;

namespace Template.Api.Services;

public interface IAuthService
{
	Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
}
