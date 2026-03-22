namespace Template.Api.Contracts.Users;

public record UpdateProfileRequest(
	string FirstName,
	string LastName,
	string? PhoneNumber,
	DateOnly DateOfBirth
);