namespace Template.Api.Contracts;

public record AuthResponse(
	string Id,
	string? Email,
	string FirstName,
	string LastName,
	DateOnly DateOfBirth,
	string Gender,
	string Token,
	int ExpiresIn
);