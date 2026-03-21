namespace Template.Api.Contracts.Auth;

public record AuthResponse(
	string Id,
	string? Email,
	string FirstName,
	string LastName,
	string? PhoneNumber,
	DateOnly DateOfBirth,
	string Gender,
	string Token,
	int ExpiresIn,
	string RefreshToken,
	DateTime RefreshTokenExpiration
);