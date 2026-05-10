namespace Template.Api.Contracts.Users;

public record UserResponse(
	string Id,
	string FirstName,
	string LastName,
	string Email,
	string? PhoneNumber,
	string DateOfBirth,
	string Gender,
	bool IsDisabled,
	IEnumerable<string> Roles
);
