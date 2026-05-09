
namespace Template.Api.Mapping;

public class MappingConfigurations : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<RegisterRequest, ApplicationUser>()
			.Map(dest => dest.UserName, src => src.Email)
			.Map(dest => dest.DateOfBirth, src => src.DateOfBirth.ToDateTime(TimeOnly.MinValue));

		config.NewConfig<ApplicationUser, UserProfileResponse>()
				.Map(dest => dest.Gender, src => src.Gender.ToString())
				.Map(dest => dest.DateOfBirth, src => src.DateOfBirth.ToShortDateString())
				.Map(dest => dest.ImageUrl, src => src.ImageUrl ?? (src.Gender == Gender.Male ? DefaultAvatars.Male : DefaultAvatars.Female));

		config.NewConfig<(ApplicationUser user, IList<string> roles), UserResponse>()
				.Map(dest => dest, src => src.user)
				.Map(dest => dest.Roles, src => src.roles);

		config.NewConfig<CreateUserRequest, ApplicationUser>()
			.Map(dest => dest.UserName, src => src.Email);
	}
}