using Template.Api.Contracts.Users;

namespace Template.Api.Services;

public interface IUserService
{
	Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
	Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
	Task<Result<string>> UpdateAvatarAsync(string userId, IFormFile avatar, CancellationToken cancellationToken = default);
	Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
	Task<Result> SendChangeEmailCodeAsync(string userId, string newEmail);
	Task<Result> ConfirmEmailChangeAsync(string userId, ConfirmEmailChangeRequest request);
	Task<Result> DeleteAccountAsync(string userId, string password);              
}
