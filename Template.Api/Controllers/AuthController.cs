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

		return authorResult is null ? BadRequest("Invalid email/password") : Ok(authorResult);
	}

}
