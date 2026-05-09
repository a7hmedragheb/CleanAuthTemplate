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

	[HttpGet("{id}")]
	public async Task<IActionResult> Get([FromRoute] string id)
	{
		var result = await _userService.GetAsync(id);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}
}
