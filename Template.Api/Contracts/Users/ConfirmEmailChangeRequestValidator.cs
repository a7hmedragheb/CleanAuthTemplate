namespace Template.Api.Contracts.Users;

public class ConfirmEmailChangeRequestValidator : AbstractValidator<ConfirmEmailChangeRequest>
{
	public ConfirmEmailChangeRequestValidator()
	{
		RuleFor(x => x.NewEmail)
			.NotEmpty()
			.EmailAddress();

		RuleFor(x => x.Token)
			.NotEmpty();
	}
}