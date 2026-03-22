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
}
