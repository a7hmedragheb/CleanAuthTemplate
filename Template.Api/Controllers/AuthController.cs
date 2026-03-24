using Microsoft.AspNetCore.Mvc;

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
	public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
	{
		var result = await _authService.RegisterAsync(request, cancellationToken);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("confirm-email")]
	public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
	{
		var result = await _authService.ConfirmEmailAsync(request);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("resend-confirmation-email")]
	public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
	{
		var result = await _authService.ResendConfirmationEmailAsync(request);
		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("forget-password")]
	public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
	{
		var result = await _authService.SendResetPasswordCodeAsync(request.Email);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("verify-code")]
	public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest request)
	{
		var result = await _authService.VerifyResetCodeAsync(request.Email, request.Code);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("reset-password")]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
	{
		var result = await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPost("login-google")]
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
