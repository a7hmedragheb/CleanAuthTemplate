using Microsoft.AspNetCore.Authorization;

namespace Template.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = DefaultRoles.Admin.Name)]
public class UsersController : ControllerBase
{
	private readonly IUserService _userService;

	public UsersController(IUserService userService)
	{
		_userService = userService;
	}

	[HttpGet("")]
	public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
	{
		var users = await _userService.GetAllAsync(cancellationToken);
	
		return Ok(users);
	}
}
