using Template.Api.Contracts.Users;

namespace Template.Api.Services;

public interface IUserService
{
	Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
	Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
	Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
	Task<Result> SendChangeEmailCodeAsync(string userId, string newEmail);
	Task<Result> ConfirmEmailChangeAsync(string userId, ConfirmEmailChangeRequest request);
}
