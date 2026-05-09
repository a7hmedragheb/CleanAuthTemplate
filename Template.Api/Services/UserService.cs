using Azure;
using static System.Net.Mime.MediaTypeNames;

namespace Template.Api.Services;
public class UserService : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ApplicationDbContext _context;
	private readonly IImageService _imageService;
	private readonly ILogger<UserService> _logger;
	private readonly IEmailSender _emailSender;
	private readonly AppSettings _appSettings;
	private readonly IRoleService _roleService;

	public UserService(UserManager<ApplicationUser> userManager,
		ApplicationDbContext context,
		IImageService imageService,
		ILogger<UserService> logger,
		IEmailSender emailSender,
		IOptions<AppSettings> appSettings,
		IRoleService roleService)
	{
		_userManager = userManager;
		_context = context;
		_imageService = imageService;
		_logger = logger;
		_emailSender = emailSender;
		_appSettings = appSettings.Value;
		_roleService = roleService;
	}

	public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
				await (from u in _context.Users
					   join ur in _context.UserRoles
					   on u.Id equals ur.UserId
					   join r in _context.Roles
					   on ur.RoleId equals r.Id into roles
					   where !roles.Any(x => x.Name == DefaultRoles.Member.Name)
					   select new
					   {
						   u.Id,
						   u.Email,
						   u.FirstName,
						   u.LastName,
						   u.IsDisabled,
						   Roles = roles.Select(x => x.Name).ToList()
					   }
					   )
					   .GroupBy(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.IsDisabled })
					   .Select(u => new UserResponse
					   (
						   u.Key.Id,
						   u.Key.Email,
						   u.Key.FirstName,
						   u.Key.LastName,
						   u.Key.IsDisabled,
						   u.SelectMany(x => x.Roles)
					   ))
					   .ToListAsync(cancellationToken);

	public async Task<Result<UserResponse>> GetAsync(string userId)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
			return Result.Failure<UserResponse>(UserErrors.UserNotFound);

		var userRoles = await _userManager.GetRolesAsync(user);

		var response = (user, userRoles).Adapt<UserResponse>();

		return Result.Success(response);
	}

	public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
	{
		var emailExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

		if (emailExists)
			return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

		var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);

		if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
			return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

		var user = request.Adapt<ApplicationUser>();

		user.EmailConfirmed = true;

		var result = await _userManager.CreateAsync(user, request.Password);

		if (result.Succeeded)
		{
			await _userManager.AddToRolesAsync(user, request.Roles);

			var response = (user, request.Roles).Adapt<UserResponse>();

			return Result.Success(response);
		}

		var error = result.Errors.First();

		return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
	{
		if (await _userManager.FindByIdAsync(id) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email && x.Id != id, cancellationToken);

		if (emailIsExists)
			return Result.Failure(UserErrors.DuplicatedEmail);

		var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);

		if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
			return Result.Failure(UserErrors.InvalidRoles);

		if (user.Email != request.Email)
			user.EmailConfirmed = false;

		user = request.Adapt(user);

		var result = await _userManager.UpdateAsync(user);

		if (result.Succeeded)
		{
			await _context.UserRoles
				.Where(x => x.UserId == id)
				.ExecuteDeleteAsync(cancellationToken);

			await _userManager.AddToRolesAsync(user, request.Roles);

			return Result.Success();
		}

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> ToggleStatusAsync(string id)
	{
		if (await _userManager.FindByIdAsync(id) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		user.IsDisabled = !user.IsDisabled;

		var result = await _userManager.UpdateAsync(user);

		if (result.Succeeded)
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> Unlock(string id)
	{
		if (await _userManager.FindByIdAsync(id) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		var result = await _userManager.SetLockoutEndDateAsync(user, null);

		if (result.Succeeded)
			return Result.Success();

		var error = result.Errors.First();

		return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
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
		var phoneNumberIsExists = await _userManager.Users
			.AnyAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != userId);

		if (phoneNumberIsExists)
			return Result.Failure<UserProfileResponse>(UserErrors.DuplicatedPhoneNumber);

		await _userManager.Users
			.Where(x => x.Id == userId)
			.ExecuteUpdateAsync(setters =>
				setters
					   .SetProperty(x => x.FirstName, request.FirstName)
					   .SetProperty(x => x.LastName, request.LastName)
					   .SetProperty(x => x.PhoneNumber, request.PhoneNumber)
					   .SetProperty(x => x.DateOfBirth, request.DateOfBirth.ToDateTime(TimeOnly.MinValue))
					   .SetProperty(x => x.Gender, request.Gender)
			);

		return Result.Success();
	}

	public async Task<Result> UpdateAvatarAsync(string userId, IFormFile avatar, CancellationToken cancellationToken = default)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken: cancellationToken) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		if (!string.IsNullOrEmpty(user.ImagePublicId))
			await _imageService.DeleteAsync(user.ImagePublicId);

		var uploadResult = await _imageService.UploadAsync(avatar, "Users", hasThumbnail: true, cancellationToken);

		user.ImageUrl = uploadResult.ImageUrl;
		user.ImageThumbnailUrl = uploadResult.ThumbnailUrl;
		user.ImagePublicId = uploadResult.PublicId;

		await _userManager.UpdateAsync(user);

		_logger.LogInformation("Avatar updated for user {UserId}", userId);

		return Result.Success();
	}

	public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		var hasPassword = await _userManager.HasPasswordAsync(user);

		if (!hasPassword)
			return Result.Failure(UserErrors.GoogleAccountCannotResetPassword);

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

		var emailExists = await _userManager.Users.AnyAsync(x => x.Email == newEmail);

		if (emailExists)
			return Result.Failure(UserErrors.DuplicatedEmail);

		var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

		var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

		await SendChangeEmailAsync(user, newEmail, encodedToken);

		//_logger.LogInformation("Email change link sent to {NewEmail} for user {UserId}", newEmail, userId);

		return Result.Success();
	}

	public async Task<Result> ConfirmEmailChangeAsync(string userId, ConfirmEmailChangeRequest request)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		string decodedToken;

		try
		{
			decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
		}
		catch (FormatException)
		{
			return Result.Failure(UserErrors.InvalidCode);
		}

		var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, decodedToken);

		if (!result.Succeeded)
		{
			var error = result.Errors.First();
			return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		await _userManager.SetUserNameAsync(user, request.NewEmail);

		//_logger.LogInformation("Email changed successfully for user {UserId}", userId);

		return Result.Success();
	}

	public async Task<Result> DeleteAccountAsync(string userId, string password)
	{
		if (await _userManager.Users.Include(u => u.RefreshTokens).SingleOrDefaultAsync(x => x.Id == userId) is not { } user)
			return Result.Failure(UserErrors.UserNotFound);

		var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

		if (!isPasswordValid)
			return Result.Failure(UserErrors.InvalidPassword);

		//  Logical Delete
		user.IsDisabled = true;

		//  Revoke Tokens
		foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
			token.RevokedOn = DateTime.UtcNow;

		await _userManager.UpdateAsync(user);

		return Result.Success();
	}

	private async Task SendChangeEmailAsync(ApplicationUser user, string newEmail, string encodedToken)
	{
		var changeEmailLink =
		$"{_appSettings.FrontendBaseUrl}/confirm-email-change" +
			$"?userId={user.Id}" +
			$"&email={Uri.EscapeDataString(newEmail)}" +
			$"&token={encodedToken}";

		//_logger.LogInformation("Email change link for {UserId}: {Link}", user.Id, changeEmailLink);

		var emailBody = await EmailBodyBuilder.GenerateEmailBody(TemplateConsts.ChangeEmail,
			new Dictionary<string, string>
			{
				{ "{{FirstName}}", user.FirstName },
				{ "{{ChangeEmailLink}}", changeEmailLink }
			}
		);

		BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(newEmail, "🔐 Template: Confirm Email Change", emailBody));
	}
}