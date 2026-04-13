using System.Security.Cryptography;

namespace Template.Api.Services;
public class AuthService : IAuthService
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IJwtProvider _jwtProvider;
	private readonly ILogger<AuthService> _logger;
	private readonly IEmailSender _emailSender;
	private readonly IGoogleAuthService _googleAuthService;
	private readonly AppSettings _appSettings;


	private readonly int _refreshTokenExpiryDays = 14;
	public AuthService(ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			IJwtProvider jwtProvider,
			ILogger<AuthService> logger,
			IEmailSender emailSender,
			IGoogleAuthService googleAuthService,
			IOptions<AppSettings> appSettings)
	{
		_context = context;
		_userManager = userManager;
		_jwtProvider = jwtProvider;
		_logger = logger;
		_emailSender = emailSender;
		_googleAuthService = googleAuthService;
		_appSettings = appSettings.Value;
	}

	public async Task<Result<AuthResult>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

		if (user is null)
			return Result.Failure<AuthResult>(UserErrors.InvalidCredentials);

		if (!user.EmailConfirmed)
			return Result.Failure<AuthResult>(UserErrors.EmailNotConfirmed);

		var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

		if (!isPasswordValid)
			return Result.Failure<AuthResult>(UserErrors.InvalidCredentials);

		var (token, expiresIn) = _jwtProvider.GenerateToken(user);

		var refreshToken = GenerateRefreshToken();
		var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);


		user.RefreshTokens.Add(new RefreshToken
		{
			Token = refreshToken,
			ExpiresOn = refreshTokenExpiration
		});

		await _userManager.UpdateAsync(user);

		var response = new AuthResult(
			new AuthResponse(
				user.Id,
				user.Email,
				user.FirstName,
				user.LastName,
				user.PhoneNumber,
				DateOnly.FromDateTime(user.DateOfBirth),
				user.Gender.ToString()!,
				token,
				expiresIn
			),
			refreshToken,
			refreshTokenExpiration
		);

		return Result.Success(response);
	}

	public async Task<Result<AuthResult>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
	{
		var userId = _jwtProvider.ValidateToken(token);

		if (userId is null)
			return Result.Failure<AuthResult>(UserErrors.InvalidJwtToken);

		var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
			return Result.Failure<AuthResult>(UserErrors.InvalidJwtToken);

		var userRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken && t.IsActive);

		if (userRefreshToken is null)
			return Result.Failure<AuthResult>(UserErrors.InvalidRefreshToken);


		userRefreshToken.RevokedOn = DateTime.UtcNow;

		var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
		var newRefreshToken = GenerateRefreshToken();
		var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

		user.RefreshTokens.Add(new RefreshToken
		{
			Token = newRefreshToken,
			ExpiresOn = refreshTokenExpiration
		});

		await _userManager.UpdateAsync(user);

		var response = new AuthResult(
			new AuthResponse(
				user.Id,
				user.Email,
				user.FirstName,
				user.LastName,
				user.PhoneNumber,
				DateOnly.FromDateTime(user.DateOfBirth),
				user.Gender.ToString()!,
				newToken,
				expiresIn
			),
			newRefreshToken,
			refreshTokenExpiration
		);

		return Result.Success(response);
	}

	public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
	{
		var userId = _jwtProvider.ValidateToken(token);

		if (userId is null)
			return Result.Failure(UserErrors.InvalidJwtToken);


		var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

		if (user is null)
			return Result.Failure(UserErrors.InvalidJwtToken);

		var userRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken && t.IsActive);

		if (userRefreshToken is null)
			return Result.Failure(UserErrors.InvalidRefreshToken);

		userRefreshToken.RevokedOn = DateTime.UtcNow;

		await _userManager.UpdateAsync(user);

		return Result.Success();
	}

	public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
	{
		var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

		if (emailIsExists)
			return Result.Failure(UserErrors.DuplicatedEmail);

		var phoneNumberIsExists = await _userManager.Users.AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);

		if (phoneNumberIsExists)
			return Result.Failure(UserErrors.DuplicatedPhoneNumber);

		var user = request.Adapt<ApplicationUser>();

		var result = await _userManager.CreateAsync(user, request.Password);

		if (!result.Succeeded)
		{
			var error = result.Errors.First();
			return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		await SendConfirmationEmailAsync(user);

		return Result.Success();
	}

	public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
	{
		if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
			return Result.Failure(UserErrors.InvalidCode);

		if (user.EmailConfirmed)
			return Result.Failure(UserErrors.DuplicatedConfirmation);

		string code;
		try
		{
			code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
		}
		catch (FormatException)
		{
			return Result.Failure(UserErrors.InvalidCode);
		}

		var result = await _userManager.ConfirmEmailAsync(user, code);

		if (!result.Succeeded)
		{
			var error = result.Errors.First();
			return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		await SendWelcomeEmailAsync(user);

		return Result.Success();
	}

	public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(u => u.Email == request.Email) is not { } user)
			return Result.Success();

		if (user.EmailConfirmed)
			return Result.Failure(UserErrors.DuplicatedConfirmation);

		await SendConfirmationEmailAsync(user);

		return Result.Success();
	}

	public async Task<Result<AuthResult>> GoogleLoginAsync(string idToken, CancellationToken cancellationToken = default)
	{
		var payload = await _googleAuthService.ValidateGoogleTokenAsync(idToken);

		if (payload is null)
			return Result.Failure<AuthResult>(UserErrors.InvalidGoogleToken);

		var user = await _userManager.Users
		.IgnoreQueryFilters()
		.SingleOrDefaultAsync(u => u.Email == payload.Email, cancellationToken);

		if (user is not null && user.IsDeleted)
			return Result.Failure<AuthResult>(UserErrors.UserNotFound);

		if (user is null)
		{
			user = new ApplicationUser
			{
				Email = payload.Email,
				UserName = payload.Email,
				FirstName = payload.GivenName ?? string.Empty,
				LastName = payload.FamilyName ?? string.Empty,
				EmailConfirmed = true,
				Gender = null,
				DateOfBirth = DateTime.MinValue
			};

			var createResult = await _userManager.CreateAsync(user);

			if (!createResult.Succeeded)
			{
				var error = createResult.Errors.First();
				return Result.Failure<AuthResult>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
			}

			await SendWelcomeEmailAsync(user);
		}

		var (token, expiresIn) = _jwtProvider.GenerateToken(user);
		var refreshToken = GenerateRefreshToken();
		var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

		user.RefreshTokens.Add(new RefreshToken
		{
			Token = refreshToken,
			ExpiresOn = refreshTokenExpiration
		});

		await _userManager.UpdateAsync(user);

		var response = new AuthResult(
			new AuthResponse(
				user.Id,
				user.Email,
				user.FirstName,
				user.LastName,
				user.PhoneNumber,
				DateOnly.FromDateTime(user.DateOfBirth),
				user.Gender?.ToString() ?? string.Empty,
				token,
				expiresIn
			),
			refreshToken,
			refreshTokenExpiration
		);

		return Result.Success(response);
	}

	public async Task<Result> SendResetPasswordCodeAsync(string email)
	{
		if (await _userManager.Users.SingleOrDefaultAsync(u => u.Email == email) is not { } user)
			return Result.Success();

		//Google Account
		var hasPassword = await _userManager.HasPasswordAsync(user);

		if (!hasPassword)
			return Result.Failure(UserErrors.GoogleAccountCannotResetPassword);

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

		var code = GenerateVerificationCode();
		var codeHash = ComputeSha256Hash(code + user.SecurityStamp);

		var entity = new PasswordResetCode
		{
			UserId = user.Id,
			CodeHash = codeHash,
			IdentityToken = encodedToken,
			ExpiresAt = DateTime.UtcNow.AddMinutes(ResetPasswordConsts.ExpiryMinutes),
		};

		_context.PasswordResetCodes.Add(entity);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Password reset code for {Email}: {Code} expires {Expiry}", user.Email, code, entity.ExpiresAt);

		//send email with 'code' (do not log in production)

		await SendResetPasswordEmailAsync(user, code);

		return Result.Success();
	}

	public async Task<Result> VerifyResetCodeAsync(string email, string code)
	{
		var (user, resetEntry) = await ValidateResetCodeAsync(email, code);

		if (user is null)
			return Result.Failure(UserErrors.InvalidCode);

		if (resetEntry is null)
			return Result.Failure(UserErrors.InvalidCode);

		_logger.LogInformation("Password reset code verified for user {UserId}", user.Id);

		return Result.Success();
	}

	public async Task<Result> ResetPasswordAsync(string email, string code, string newPassword)
	{
		var (user, resetEntry) = await ValidateResetCodeAsync(email, code);

		if (user is null)
			return Result.Failure(UserErrors.InvalidCode);

		if (resetEntry is null)
			return Result.Failure(UserErrors.InvalidCode);

		string identityToken;
		try
		{
			var tokenBytes = WebEncoders.Base64UrlDecode(resetEntry.IdentityToken);
			identityToken = Encoding.UTF8.GetString(tokenBytes);
		}
		catch
		{
			return Result.Failure(UserErrors.ExpiredCode with { Description = "Malformed reset token" });
		}

		var resetResult = await _userManager.ResetPasswordAsync(user, identityToken, newPassword);

		if (!resetResult.Succeeded)
		{
			var error = resetResult.Errors.First();
			return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
		}

		// Remove all old codes
		var activeResetCodes = await _context.PasswordResetCodes
			.Where(x => x.UserId == user.Id && x.UsedAt == null)
			.ToListAsync();

		foreach (var resetCode in activeResetCodes)
			resetCode.UsedAt = DateTime.UtcNow;

		await _userManager.UpdateSecurityStampAsync(user);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Password reset completed for user {UserId}", user.Id);
		return Result.Success();
	}

	private async Task<(ApplicationUser? User, PasswordResetCode? ResetEntry)> ValidateResetCodeAsync(string email, string code)
	{
		var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == email);
		if (user is null) return (null, null);

		var resetEntry = await _context.PasswordResetCodes
			.Where(x => x.UserId == user.Id && x.UsedAt == null && x.ExpiresAt > DateTime.UtcNow)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync();

		if (resetEntry is null) return (user, null);

		var providedHash = ComputeSha256Hash(code + user.SecurityStamp);

		if (!string.Equals(providedHash, resetEntry.CodeHash, StringComparison.Ordinal))
		{
			resetEntry.Attempts++;
			if (resetEntry.Attempts >= ResetPasswordConsts.MaxAttempts)
				resetEntry.UsedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return (user, null);
		}

		return (user, resetEntry);
	}

	private static readonly char[] _allowedNumber = AllowedNumber._allowedNumber;
	private static string GenerateVerificationCode(int length = 6)
	{
		var codeDigits = new char[length];

		do
		{
			var randomBytes = RandomNumberGenerator.GetBytes(length);

			for (int i = 0; i < length; i++)
			{
				if (randomBytes[i] < 256 - (256 % _allowedNumber.Length))
					codeDigits[i] = _allowedNumber[randomBytes[i] % _allowedNumber.Length];
			}

		} while (codeDigits.Contains('\0'));

		return new string(codeDigits);
	}

	private static string ComputeSha256Hash(string input)
	{
		var bytes = Encoding.UTF8.GetBytes(input);
		var hashed = SHA256.HashData(bytes);
		return Convert.ToBase64String(hashed);
	}

	private static string GenerateRefreshToken()
	{
		var refreshToken = RandomNumberGenerator.GetBytes(64);

		return Convert.ToBase64String(refreshToken);
	}

	private async Task SendConfirmationEmailAsync(ApplicationUser user)
	{
		var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

		var confirmationLink = $"{_appSettings.FrontendBaseUrl}/confirm-email?userId={user.Id}&code={encodedCode}";

		//  Log for test
		_logger.LogInformation("Confirmation Link for {Email}: userId={UserId} code={Code}", user.Email, user.Id, encodedCode);

		var emailBody = await EmailBodyBuilder.GenerateEmailBody(TemplateConsts.EmailConfirmation,
			new Dictionary<string, string>
			{
				{ "{{FirstName}}", user.FirstName },
				{ "{{ConfirmationLink}}", confirmationLink }
			}
		);

		BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Template: Confirm Your Email", emailBody));

		_logger.LogInformation("Confirmation email enqueued for {Email}", user.Email);
	}

	private async Task SendWelcomeEmailAsync(ApplicationUser user)
	{
		var emailBody = await EmailBodyBuilder.GenerateEmailBody(TemplateConsts.Welcome,
			new Dictionary<string, string>
			{
				{ "{{FirstName}}", user.FirstName }
			}
		);

		BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "🎉 Welcome to Template", emailBody));

		_logger.LogInformation("Welcome email enqueued for {Email}", user.Email);
	}

	private async Task SendResetPasswordEmailAsync(ApplicationUser user, string code)
	{
		var emailBody = await EmailBodyBuilder.GenerateEmailBody(TemplateConsts.ForgetPassword,
			new Dictionary<string, string>
			{
				{ "{{Code}}", code },
				{ "{{FirstName}}", user.FirstName }
			}
		);

		BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "🔐 Template: Change Password", emailBody));

		_logger.LogInformation("Reset password email enqueued for {Email}", user.Email);
	}
}
