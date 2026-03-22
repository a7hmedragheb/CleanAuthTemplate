using Template.Api.Contracts.Users;

namespace Template.Api.Services;

public interface IUserService
{
	Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
	Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
