using Microsoft.AspNetCore.Identity;
using Template.Api.Contracts.Users;

namespace Template.Api.Services;

public class UserService : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager;
	public UserService(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
			return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		var response = user.Adapt<UserProfileResponse>();

		return Result.Success(response);
	}

	public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
	{
		//	if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
		//		return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

		await _userManager.Users
			.Where(x => x.Id == userId)
			.ExecuteUpdateAsync(setters =>
				setters
					   .SetProperty(x => x.FirstName, request.FirstName)
					   .SetProperty(x => x.LastName, request.LastName)
					   .SetProperty(x => x.PhoneNumber, request.PhoneNumber)
					   .SetProperty(x => x.DateOfBirth, request.DateOfBirth.ToDateTime(TimeOnly.MinValue))
			);

		return Result.Success();
	}

	public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
	{
		var user = await _userManager.FindByIdAsync(userId);

		var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

		if (result.Succeeded)
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}
}