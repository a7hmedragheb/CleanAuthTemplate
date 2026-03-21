namespace Template.Api.Contracts.Auth;
public record VerifyResetCodeRequest(
	string Email,
	string Code
);