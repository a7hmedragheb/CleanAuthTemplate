namespace Template.Api.Contracts.Auth;


public record RefreshTokenRequest(
	string Token,
	string RefreshToken
);