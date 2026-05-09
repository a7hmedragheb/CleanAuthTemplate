using Template.Api.Contracts.Roles;

namespace Template.Api.Services;

public interface IRoleService
{
	Task<IEnumerable<RoleResponse>> GetAllAsync(bool? includeDisabled = false, CancellationToken cancellationToken = default);
	Task<Result<RoleResponse>> GetAsync(string Id);
	Task<Result<RoleResponse>> AddAsync(RoleRequest request);
	Task<Result> UpdateAsync(string id, RoleRequest request);
	Task<Result> ToggleStatusAsync(string id);
}