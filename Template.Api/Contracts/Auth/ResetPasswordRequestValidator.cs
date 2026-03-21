using FluentValidation;
using Template.Api.Abstractions.Consts;

namespace Template.Api.Contracts.Auth;
public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
	public ResetPasswordRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress();

		RuleFor(x => x.Code)
			.NotEmpty()
			.MaximumLength(6);

		RuleFor(x => x.NewPassword)
				.NotEmpty()
				.Matches(RegexPatterns.Password)
				.WithMessage("Password should be at least 8 digits and should contains Lowercase, Uppercase and NonAlphanumeric");

		RuleFor(x => x.ConfirmPassword)
			.NotEmpty()
			.Equal(x => x.NewPassword);
	}
}