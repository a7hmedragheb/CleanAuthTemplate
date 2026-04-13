namespace Template.Api.Contracts.Auth;

public class GoogleAuthRequestValidator : AbstractValidator<GoogleAuthRequest>
{
	public GoogleAuthRequestValidator()
	{
		RuleFor(x => x.IdToken)
			.NotEmpty();
	}
}
