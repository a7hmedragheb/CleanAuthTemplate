using FluentValidation;
using Template.Api.Abstractions.Consts;

namespace Template.Api.Contracts.Auth;

public class VerifyResetCodeRequestValidator : AbstractValidator<VerifyResetCodeRequest>
{
	public VerifyResetCodeRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress();

		RuleFor(x => x.Code)
			.NotEmpty()
			.Length(6)
			.Matches(RegexPatterns.NumbersOnly)
			.WithMessage("Code must be 6 numbers only.");
	}
}

