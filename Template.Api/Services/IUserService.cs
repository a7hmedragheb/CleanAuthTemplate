namespace Template.Api.Services;

public interface IUserService
{
	Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<Result<UserResponse>> GetAsync(string userId);

	Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
	Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
	Task<Result> UpdateAvatarAsync(string userId, IFormFile avatar, CancellationToken cancellationToken = default);
	Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
	Task<Result> SendChangeEmailCodeAsync(string userId, string newEmail);
	Task<Result> ConfirmEmailChangeAsync(string userId, ConfirmEmailChangeRequest request);
	Task<Result> DeleteAccountAsync(string userId, string password);
}
