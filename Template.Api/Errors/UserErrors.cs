namespace Template.Api.Errors;


public record UserErrors
{
	public static readonly Error InvalidCredentials =
		  new("User.InvalidCredentials", "Invalid email/password", StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidJwtToken =
		new("User.InvalidJwtToken", "Invalid Jwt token", StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidRefreshToken =
		new("User.InvalidRefreshToken", "Invalid refresh token", StatusCodes.Status401Unauthorized);

	public static readonly Error DuplicatedEmail =
		new("User.DuplicatedEmail", "Another user with the same email is already exists", StatusCodes.Status409Conflict);

	public static readonly Error DuplicatedPhoneNumber =
		new("User.DuplicatedPhoneNumber", "Another user with the same PhoneNumber is already exists", StatusCodes.Status409Conflict);

	public static readonly Error InvalidCode =
	  new("Code.Invalid", "Invalid email or code", StatusCodes.Status400BadRequest);

	public static readonly Error CodeReset =
	  new("Code.Invalid", "No valid reset code found or it expired", StatusCodes.Status400BadRequest);

	public static readonly Error UserNotFound =
	  new("Code.UserNotFound", "User not found", StatusCodes.Status404NotFound);
}
