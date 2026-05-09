using Microsoft.AspNetCore.Authorization;
using Template.Api.Contracts.Roles;


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

	[HttpGet("{id}")]
	public async Task<IActionResult> Get([FromRoute] string id)
	{
		var result = await _roleService.GetAsync(id);

		return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
	}

	[HttpPost("")]
	public async Task<IActionResult> Add([FromBody] RoleRequest request)
	{
		var result = await _roleService.AddAsync(request);

		return result.IsSuccess ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value) : result.ToProblem();
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleRequest request)
	{
		var result = await _roleService.UpdateAsync(id, request);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}
}