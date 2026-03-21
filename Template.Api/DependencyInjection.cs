using FluentValidation;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;
using System.Text;
using Template.Api.Authentication;

namespace Template.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddControllers();

		var ConnectionString = configuration.GetConnectionString("DefaultConnection") ??
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(ConnectionString));


		services.AddScoped<IAuthService, AuthService>();



		services
			.AddMapsterConfig()
			.AddFluentValidationConfig()
			.AddAuthorConfig(configuration);


		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();

		return services;
	}

	private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
	{
		var mappingConfig = TypeAdapterConfig.GlobalSettings;
		mappingConfig.Scan(Assembly.GetExecutingAssembly());

		services.AddSingleton<IMapper>(new Mapper(mappingConfig));

		return services;

	}

	private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
	{
		services
			.AddFluentValidationAutoValidation()
			.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

		return services;
	}

	private static IServiceCollection AddAuthorConfig(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddIdentity<ApplicationUser, IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders(); 

		services.AddScoped<IJwtProvider, JwtProvider>();

		services.AddOptions<JwtOptions>()
			.BindConfiguration(JwtOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		var JwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(o =>
		{
			o.SaveToken = true;
			o.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings?.key!)),
				ValidIssuer = JwtSettings?.Issuer,
				ValidAudience = JwtSettings?.Audience,
			};
		});

		services.Configure<IdentityOptions>(options =>
		{
			options.Password.RequiredLength = 8;
			//options.SignIn.RequireConfirmedEmail = true;
			options.User.RequireUniqueEmail = true;
		});

		return services;
	}
}