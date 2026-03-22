using FluentValidation;

namespace Template.Api.Contracts.Users;

public class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
	public ChangeEmailRequestValidator()
	{
		RuleFor(x => x.NewEmail)
			.NotEmpty()
			.EmailAddress();
	}
}
