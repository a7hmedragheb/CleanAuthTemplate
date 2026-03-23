public record UserErrors
{
	public static readonly Error InvalidCredentials =
		new("User.InvalidCredentials", "Invalid email/password.", StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidJwtToken =
		new("User.InvalidJwtToken", "Invalid JWT token.", StatusCodes.Status401Unauthorized);

	public static readonly Error InvalidRefreshToken =
		new("User.InvalidRefreshToken", "Invalid refresh token.", StatusCodes.Status401Unauthorized);

	public static readonly Error DuplicatedEmail =
		new("User.DuplicatedEmail", "A user with the same email already exists.", StatusCodes.Status409Conflict);

	public static readonly Error DuplicatedPhoneNumber =
		new("User.DuplicatedPhoneNumber", "A user with the same phone number already exists.", StatusCodes.Status409Conflict);

	public static readonly Error InvalidCode =
		new("Code.InvalidCode", "Invalid email or code.", StatusCodes.Status400BadRequest);

	public static readonly Error ExpiredCode =
		new("Code.ExpiredCode", "No valid code found or it has expired.", StatusCodes.Status400BadRequest);

	public static readonly Error UserNotFound =
		new("User.UserNotFound", "User not found.", StatusCodes.Status404NotFound);

	public static readonly Error InvalidPassword =
		new("User.InvalidPassword", "Invalid password.", StatusCodes.Status400BadRequest);

	public static readonly Error InvalidGoogleToken =
		new("User.InvalidGoogleToken", "Invalid Google token.", StatusCodes.Status401Unauthorized);

	public static readonly Error GoogleAccountCannotResetPassword =
		new("User.GoogleAccount", "This account uses Google login. Please sign in with Google.", StatusCodes.Status400BadRequest);

	public static readonly Error EmailNotConfirmed =
		new("User.EmailNotConfirmed", "Email is not confirmed.", StatusCodes.Status401Unauthorized);

	public static readonly Error DuplicatedConfirmation =
		new("User.DuplicatedConfirmation", "Email is already confirmed.", StatusCodes.Status400BadRequest);
}