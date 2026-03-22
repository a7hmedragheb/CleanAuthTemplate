using FluentValidation;

namespace Template.Api.Contracts.Users;

public class ConfirmEmailChangeRequestValidator : AbstractValidator<ConfirmEmailChangeRequest>
{
	public ConfirmEmailChangeRequestValidator()
	{
		RuleFor(x => x.NewEmail)
			.NotEmpty()
			.EmailAddress();

		RuleFor(x => x.Code)
			.NotEmpty()
			.Length(6)
			.Matches(RegexPatterns.NumbersOnly)
			.WithMessage("Code must be 6 numbers only.");
	}
}