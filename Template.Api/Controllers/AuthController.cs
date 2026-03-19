using Microsoft.AspNetCore.Mvc;
using Template.Api.Contracts.Auth;

namespace Template.Api.Controllers;
[Route("api/[controller]")]
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
		var authorResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

		return authorResult.IsSuccess ? Ok(authorResult.Value) : authorResult.ToProblem();
	}

	[HttpPost("refresh")]
	public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
	{
		var authorResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

		return authorResult.IsSuccess ? Ok(authorResult.Value) : authorResult.ToProblem();
	}

}
