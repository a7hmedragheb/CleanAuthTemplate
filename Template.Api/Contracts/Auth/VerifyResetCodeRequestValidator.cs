using FluentValidation;

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
			.MaximumLength(6);
	}
}

