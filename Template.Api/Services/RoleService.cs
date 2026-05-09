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

	public async Task<Result<RoleResponse>> AddAsync(RoleRequest request)
	{
		var roleIsExists = await _roleManager.RoleExistsAsync(request.Name);

		if (roleIsExists)
			return Result.Failure<RoleResponse>(RoleErrors.DuplicatedRole);

		var role = new ApplicationRole
		{
			Name = request.Name,
			ConcurrencyStamp = Guid.CreateVersion7().ToString()
		};

		var result = await _roleManager.CreateAsync(role);

		if (result.Succeeded)
		{
			var response = new RoleResponse(role.Id, role.Name, role.IsDeleted);

			return Result.Success(response);
		}

		var error = result.Errors.First();

		return Result.Failure<RoleResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> UpdateAsync(string id, RoleRequest request)
	{
		if (await _roleManager.FindByIdAsync(id) is not { } role)
			return Result.Failure(RoleErrors.RoleNotFound);

		var roleIsExists = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id);

		if (roleIsExists)
			return Result.Failure(RoleErrors.DuplicatedRole);

		role.Name = request.Name;

		var result = await _roleManager.UpdateAsync(role);

		if (result.Succeeded)
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}
}
