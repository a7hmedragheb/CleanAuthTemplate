namespace Template.Api.Contracts.Users;
public record CreateUserRequest(
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	DateOnly DateOfBirth,
	Gender? Gender,
	string Password,
	IList<string> Roles
);