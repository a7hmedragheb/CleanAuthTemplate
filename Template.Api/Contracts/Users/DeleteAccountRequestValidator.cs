namespace Template.Api.Contracts.Users;

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
	public DeleteAccountRequestValidator()
	{
		RuleFor(x => x.Password)
			.NotEmpty();
	}
}