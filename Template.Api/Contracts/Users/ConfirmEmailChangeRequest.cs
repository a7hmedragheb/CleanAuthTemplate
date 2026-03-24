namespace Template.Api.Contracts.Users;

public record ConfirmEmailChangeRequest(
	string NewEmail,
	string Token
);
