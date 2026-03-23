using Template.Api.Contracts.Auth;

namespace Template.Api.Services;

public interface IAuthService
{
	Task<Result<AuthResult>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
	Task<Result<AuthResult>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
	Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
	Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
	Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);

	Task<Result> SendResetPasswordCodeAsync(string email);
	Task<Result> VerifyResetCodeAsync(string email, string code);
	Task<Result> ResetPasswordAsync(string email, string code, string newPassword);
	Task<Result<AuthResult>> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default);
}
