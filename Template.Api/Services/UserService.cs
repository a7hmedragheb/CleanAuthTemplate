using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Template.Api.Contracts.Users;

namespace Template.Api.Services;

public class UserService : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<AuthService> _logger;
	private readonly IEmailSender _emailSender;
	public UserService(UserManager<ApplicationUser> userManager,
		ILogger<AuthService> logger,
		IEmailSender emailSender)
	{
		_userManager = userManager;
		_logger = logger;
		_emailSender = emailSender;
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

	public async Task<Result> SendChangeEmailCodeAsync(string userId, string newEmail)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == newEmail);
		
		if (emailIsExists)
			return Result.Failure(UserErrors.DuplicatedEmail);

		var code = SecurityHelper.GenerateVerificationCode();
		var codeHash = SecurityHelper.ComputeSha256Hash(code + user.SecurityStamp);

		user.PendingEmail = newEmail;
		user.EmailChangeCodeHash = codeHash;
		user.EmailChangeCodeExpiresAt = DateTime.UtcNow.AddMinutes(ResetPasswordConsts.ExpiryMinutes);

		await _userManager.UpdateAsync(user);

		var emailBody = await EmailBodyBuilder.GenerateEmailBody(TemplateConsts.ChangeEmail,
			new Dictionary<string, string>
			{
				{ "{{FirstName}}", user.FirstName },
				{ "{{Code}}", code }
			}
		);

		await _emailSender.SendEmailAsync(newEmail, "🔐 Template: Change Email", emailBody);

		_logger.LogInformation("Email change code sent to {NewEmail} for user {UserId}", newEmail, userId);

		return Result.Success();
	}

	public async Task<Result> ConfirmEmailChangeAsync(string userId, ConfirmEmailChangeRequest request)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		if (user.PendingEmail != request.NewEmail)
			return Result.Failure(UserErrors.InvalidCode);

		if (user.EmailChangeCodeExpiresAt < DateTime.UtcNow)
			return Result.Failure(UserErrors.InvalidCode);

		var providedHash = SecurityHelper.ComputeSha256Hash(request.Code + user.SecurityStamp);

		if (!string.Equals(providedHash, user.EmailChangeCodeHash, StringComparison.Ordinal))
			return Result.Failure(UserErrors.InvalidCode);

		var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
		var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, token);

		if (!result.Succeeded)
		{
			var error = result.Errors.First();
			return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		// Remove Pending Email Data
		user.PendingEmail = null;
		user.EmailChangeCodeHash = null;
		user.EmailChangeCodeExpiresAt = null;

		await _userManager.UpdateAsync(user);

		_logger.LogInformation("Email changed successfully for user {UserId}", userId);

		return Result.Success();
	}

	public async Task<Result> DeleteAccountAsync(string userId, string password)
	{
		if (await _userManager.FindByIdAsync(userId) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

		if (!isPasswordValid)
			return Result.Failure(UserErrors.InvalidPassword);

		//  Logical Delete
		user.IsDeleted = true;
		user.DeletedAt = DateTime.UtcNow;

		//  Revoke Tokens
		foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
			token.RevokedOn = DateTime.UtcNow;

		await _userManager.UpdateAsync(user);

		_logger.LogInformation("Account soft deleted for user {UserId}", userId);

		return Result.Success();
	}
}