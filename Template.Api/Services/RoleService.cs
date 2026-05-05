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
}
