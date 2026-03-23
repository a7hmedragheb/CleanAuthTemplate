using FluentValidation;

namespace Template.Api.Contracts.Users;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
	public UpdateProfileRequestValidator()
	{
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


		RuleFor(x => x.PhoneNumber)
					.NotEmpty()
					.Matches(RegexPatterns.PhoneNumber)
					.WithMessage("Invalid mobile number.");

		RuleFor(x => x.DateOfBirth)
					.NotEmpty()
					.LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));


		RuleFor(x => x.Gender)
					.NotNull()
					.WithMessage("Gender is required.")
					.IsInEnum()
					.WithMessage("Invalid gender value. Please choose: 0 for Male or 1 for Female.");
	}
}

