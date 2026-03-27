namespace Template.Api.Contracts.Users;

public record UserProfileResponse(
	string Id,
	string FirstName,
	string LastName,
	string? Email,
	string? PhoneNumber,
	DateOnly DateOfBirth,
	string Gender,
	string? ImageUrl,
	string? ImageThumbnailUrl
);