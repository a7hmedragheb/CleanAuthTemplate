using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Template.Api.Authentication;
using Template.Api.Contracts.Auth;

namespace Template.Api.Services;

public class AuthService : IAuthService
{

	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IJwtProvider _jwtProvider;
	private readonly int _refreshTokenExpiryDays = 14;
	public AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider)
	{
		_userManager = userManager;
		_jwtProvider = jwtProvider;
	}

	public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByEmailAsync(email);

		if (user is null)
			return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

		var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

		if (!isPasswordValid)
			return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

		var (token, expiresIn) = _jwtProvider.GenerateToken(user);

		var refreshToken = GenerateRefreshToken();
		var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);


		user.RefreshTokens.Add(new RefreshToken
		{
			Token = refreshToken,
			ExpiresOn = refreshTokenExpiration
		});

		await _userManager.UpdateAsync(user);


		var response = new AuthResponse(
			user.Id,
			user.Email,
			user.FirstName,
			user.LastName,
			user.PhoneNumber,
			DateOnly.FromDateTime(user.DateOfBirth),
			user.Gender.ToString(),
			token,
			expiresIn,
			refreshToken,
			refreshTokenExpiration
		);

		return Result.Success(response);
	}

	public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
	{
		var userId = _jwtProvider.ValidateToken(token);

		if (userId is null)
			return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

		var user = await _userManager.FindByIdAsync(userId);

		if (user is null)
			return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

		var userRefreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken && t.IsActive);

		if (userRefreshToken is null)
			return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);


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

		var response = new AuthResponse(
			user.Id,
			user.Email,
			user.FirstName,
			user.LastName,
			user.PhoneNumber,
			DateOnly.FromDateTime(user.DateOfBirth),
			user.Gender.ToString(),
			newToken,
			expiresIn,
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

	private static string GenerateRefreshToken()
	{
		var refreshToken = RandomNumberGenerator.GetBytes(64);

		return Convert.ToBase64String(refreshToken);
	}
}
