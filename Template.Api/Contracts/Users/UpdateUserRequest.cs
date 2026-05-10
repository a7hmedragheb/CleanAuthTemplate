namespace Template.Api.Contracts.Users;

public record UpdateUserRequest(
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	DateOnly DateOfBirth,
	Gender? Gender,
	IList<string> Roles
);