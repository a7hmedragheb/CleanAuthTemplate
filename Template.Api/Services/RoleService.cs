using Template.Api.Contracts.Roles;

namespace Template.Api.Services;

public class RoleService : IRoleService
{
	private readonly RoleManager<ApplicationRole> _roleManager;
	public RoleService(RoleManager<ApplicationRole> roleManager)
	{
		_roleManager = roleManager;
	}

	public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisabled = false, CancellationToken cancellationToken = default) =>
	await _roleManager.Roles
		 .Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisabled.HasValue && includeDisabled.Value)))
		 .ProjectToType<RoleResponse>()
		 .ToListAsync(cancellationToken);

	public async Task<Result<RoleResponse>> GetAsync(string id)
	{
		if (await _roleManager.FindByIdAsync(id) is not { } role)
			return Result.Failure<RoleResponse>(RoleErrors.RoleNotFound);

		var response = new RoleResponse(role.Id, role.Name!, role.IsDeleted);

		return Result.Success(response);
	}
}
