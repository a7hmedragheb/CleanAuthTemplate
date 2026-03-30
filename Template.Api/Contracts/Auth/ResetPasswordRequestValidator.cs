using FluentValidation;

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
			.Length(6)
			.Matches(RegexPatterns.NumbersOnly)
			.WithMessage("Code must be 6 numbers only.");

		RuleFor(x => x.NewPassword)
				.NotEmpty()
				.Matches(RegexPatterns.Password)
				.WithMessage("Password should be at least 8 digits and should contains Lowercase, Uppercase and NonAlphanumeric");

		RuleFor(x => x.ConfirmPassword)
			.NotEmpty()
			.Equal(x => x.NewPassword);
	}
}