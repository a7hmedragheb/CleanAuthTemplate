using FluentValidation;

namespace Template.Api.Contracts.Auth;
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
	public RegisterRequestValidator()
	{
		RuleFor(x => x.Email)
					.NotEmpty()
					.EmailAddress();

		RuleFor(x => x.FirstName)
					.NotEmpty()
					.Length(3, 250)
					.Matches(RegexPatterns.CharactersOnly_Eng)
					.WithMessage("Only English letters are allowed.");

		RuleFor(x => x.LastName)
					.NotEmpty()
					.Length(3, 250)
					.Matches(RegexPatterns.CharactersOnly_Eng)
					.WithMessage("Only English letters are allowed.");


		RuleFor(x => x.Gender)
					.NotNull()
					.WithMessage("Gender is required.")
					.IsInEnum()
					.WithMessage("Invalid gender value. Please choose: 0 for Male or 1 for Female.");

		RuleFor(x => x.PhoneNumber)
					.NotEmpty()
					.Matches(RegexPatterns.PhoneNumber)
					.WithMessage("Invalid mobile number.");

		RuleFor(x => x.DateOfBirth)
					.NotEmpty()
					.LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));

		RuleFor(x => x.Password)
					.NotEmpty()
					.Matches(RegexPatterns.Password)
					.WithMessage("Password should be at least 8 digits and should contains Lowercase, Uppercase and NonAlphanumeric");

		RuleFor(x => x.ConfirmPassword)
					.NotEmpty()
					.Equal(x => x.Password)
					.WithMessage("Confirm password must be equal password.");
	}
}