using Microsoft.AspNetCore.Authorization;


[Route("api/[controller]")]
[Authorize(Roles = DefaultRoles.Admin.Name)]
[ApiController]
public class RolesController : ControllerBase
{
	private readonly IRoleService _roleService;
	public RolesController(IRoleService roleService)
	{
		_roleService = roleService;
	}

	[HttpGet("")]
	public async Task<IActionResult> GetAll([FromQuery] bool includeDisabled, CancellationToken cancellation)
	{
		var roles = await _roleService.GetAllAsync(includeDisabled, cancellation);

		return Ok(roles);
	}
}