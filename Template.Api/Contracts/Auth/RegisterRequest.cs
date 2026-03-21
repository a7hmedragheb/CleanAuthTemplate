namespace Template.Api.Contracts.Auth;

public record RegisterRequest(
	string Email,
	string FirstName,
	string LastName,
	string PhoneNumber,
	DateOnly DateOfBirth,
	string Gender,
	string Password,
	string ConfirmPassword
);