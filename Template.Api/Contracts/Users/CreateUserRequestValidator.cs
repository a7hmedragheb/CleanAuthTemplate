namespace Template.Api.Contracts.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator()
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


		RuleFor(x => x.Password)
			.NotEmpty()
			.Matches(RegexPatterns.Password)
			.WithMessage("Password should be at least 8 digits and should contains Lowercas, NonAlpanumeric, Uppercase.");


		RuleFor(x => x.Roles)
			.NotNull()
			.NotEmpty();

		RuleFor(x => x.Roles)
			.Must(x => x.Distinct().Count() == x.Count)
			.WithMessage("You Cannot add duplicated role for the same user")
			.When(x => x.Roles != null);
	}
}