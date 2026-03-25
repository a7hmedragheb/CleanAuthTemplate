using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Template.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{

	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}
	[HttpPost("")]
	[EnableRateLimiting("AuthPolicy")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

		if (result.IsSuccess)
		{
			SetRefreshTokenInCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiration);
			return Ok(result.Value.Response);
		}

		return result.ToProblem();
	}

	[HttpPost("refresh")]
	[EnableRateLimiting("GeneralPolicy")]
	public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
	{
		var result = await _authService.GetRefreshTokenAsync(GetTokenFromHeader(), GetRefreshTokenFromCookie()!, cancellationToken);

		if (result.IsSuccess)
		{
			SetRefreshTokenInCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiration);
			return Ok(result.Value.Response);
		}

		return result.ToProblem();
	}


	[HttpPost("revoke-refresh-token")]
	[EnableRateLimiting("GeneralPolicy")]
	public async Task<IActionResult> RevokeRefreshToken(CancellationToken cancellationToken)
	{
		var result = await _authService.RevokeRefreshTokenAsync(GetTokenFromHeader(), GetRefreshTokenFromCookie()!, cancellationToken);

		if (result.IsSuccess)
		{
			Response.Cookies.Delete(CookieKeys.RefreshToken);
			return Ok();
		}

		return result.ToProblem();
	}

	[HttpPost("register")]
	[EnableRateLimiting("AuthPolicy")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.RegisterAsync(request, cancellationToken);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("confirm-email")]
	[EnableRateLimiting("GeneralPolicy")]
	public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
	{
		var result = await _authService.ConfirmEmailAsync(request);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("resend-confirmation-email")]
	[EnableRateLimiting("SensitivePolicy")]
	public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
	{
		var result = await _authService.ResendConfirmationEmailAsync(request);
		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("forget-password")]
	[EnableRateLimiting("SensitivePolicy")]
	public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
	{
		var result = await _authService.SendResetPasswordCodeAsync(request.Email);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("verify-code")]
	[EnableRateLimiting("SensitivePolicy")]
	public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest request)
	{
		var result = await _authService.VerifyResetCodeAsync(request.Email, request.Code);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("reset-password")]
	[EnableRateLimiting("SensitivePolicy")]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
	{
		var result = await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("login-google")]
	[EnableRateLimiting("AuthPolicy")]
	public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.GoogleLoginAsync(request.IdToken, cancellationToken);

		if (result.IsSuccess)
		{
			SetRefreshTokenInCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiration);
			return Ok(result.Value.Response);
		}

		return result.ToProblem();
	}

	private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
	{
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Strict,
			Expires = expires.ToLocalTime()
		};

		Response.Cookies.Append(CookieKeys.RefreshToken, refreshToken, cookieOptions);
	}

	private string? GetRefreshTokenFromCookie()
		=> Request.Cookies[CookieKeys.RefreshToken];

	private string GetTokenFromHeader()
		=> Request.Headers.Authorization.ToString().Replace(HeaderKeys.Authorization, "");
}
