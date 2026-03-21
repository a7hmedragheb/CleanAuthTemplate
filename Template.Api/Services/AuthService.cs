using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Template.Api.Authentication;
using Template.Api.Contracts.Auth;

namespace Template.Api.Services;

public class AuthService : IAuthService
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IJwtProvider _jwtProvider;
	private readonly ILogger<AuthService> _logger;

	private readonly int _refreshTokenExpiryDays = 14;
	public AuthService(ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			IJwtProvider jwtProvider,
			ILogger<AuthService> logger)
	{
		_context = context;
		_userManager = userManager;
		_jwtProvider = jwtProvider;
		_logger = logger;
	}

	public async Task<Result<AuthResult>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByEmailAsync(email);

		if (user is null)
			return Result.Failure<AuthResult>(UserErrors.InvalidCredentials);

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
				user.Gender.ToString(),
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

		var user = await _userManager.FindByIdAsync(userId);

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
				user.Gender.ToString(),
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

		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure(UserErrors.InvalidJwtToken);

		var userRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken && t.IsActive);

		if (userRefreshToken is null)
			return Result.Failure(UserErrors.InvalidRefreshToken);

		userRefreshToken.RevokedOn = DateTime.UtcNow;

		await _userManager.UpdateAsync(user);

		return Result.Success();
	}

	public async Task<Result<AuthResult>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
	{
		var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

		if (emailIsExists)
			return Result.Failure<AuthResult>(UserErrors.DuplicatedEmail);

		var phoneNumberIsExists = await _userManager.Users.AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);

		if (phoneNumberIsExists)
			return Result.Failure<AuthResult>(UserErrors.DuplicatedPhoneNumber);

		var user = request.Adapt<ApplicationUser>();

		var result = await _userManager.CreateAsync(user, request.Password);

		if (result.Succeeded)
		{
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
					user.Gender.ToString(),
					token,
					expiresIn
				),
				refreshToken,
				refreshTokenExpiration
			);

			return Result.Success(response);
		}

		var error = result.Errors.First();

		return Result.Failure<AuthResult>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
	}

	public async Task<Result> SendResetPasswordCodeAsync(string email)
	{
		if (await _userManager.FindByEmailAsync(email) is not { } user)
			return Result.Success();

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

		var code = GenerateVerificationCode(5);
		var codeHash = ComputeSha256Hash(code + user.SecurityStamp);

		var entity = new PasswordResetCode
		{
			UserId = user.Id,
			CodeHash = codeHash,
			IdentityToken = encodedToken,
			ExpiresAt = DateTime.UtcNow.AddMinutes(5),
			CreatedAt = DateTime.UtcNow,
			Used = false
		};

		_context.PasswordResetCodes.Add(entity);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Password reset code for {Email}: {Code} expires {Expiry}", user.Email, code, entity.ExpiresAt);

		// TODO: send email with 'code' (do not log in production)

		return Result.Success();
	}

	private static string GenerateRefreshToken()
	{
		var refreshToken = RandomNumberGenerator.GetBytes(64);

		return Convert.ToBase64String(refreshToken);
	}

	public static readonly char[] _allowedNumber = "0123456789".ToCharArray();

	private static string GenerateVerificationCode(int length = 5)
	{
		var chars = new char[length];

		do
		{
			var bytes = RandomNumberGenerator.GetBytes(length);
			for (int i = 0; i < length; i++)
			{
				if (bytes[i] < 256 - (256 % _allowedNumber.Length))
					chars[i] = _allowedNumber[bytes[i] % _allowedNumber.Length];
			}
		} while (chars.Contains('\0'));

		return new string(chars);
	}
	private static string ComputeSha256Hash(string input)
	{
		var bytes = Encoding.UTF8.GetBytes(input);
		var hashed = SHA256.HashData(bytes);
		return Convert.ToBase64String(hashed);
	}
}
