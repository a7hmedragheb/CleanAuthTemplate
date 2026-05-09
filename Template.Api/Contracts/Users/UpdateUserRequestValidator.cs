namespace Template.Api.Contracts.Users;
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
	public UpdateUserRequestValidator()
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

		RuleFor(x => x.Roles)
			.NotNull()
			.NotEmpty();

		RuleFor(x => x.Roles)
			.Must(x => x.Distinct().Count() == x.Count)
			.WithMessage("You Cannot add duplicated role for the same user")
			.When(x => x.Roles != null);
	}
}