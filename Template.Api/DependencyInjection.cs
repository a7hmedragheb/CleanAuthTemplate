using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;
using System.Threading.RateLimiting;


namespace Template.Api;
public static class DependencyInjection
{
	public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddControllers();

		var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();

		services.AddCors(options =>
			options.AddDefaultPolicy(builder =>
				builder
					//.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.WithOrigins(allowedOrigins!)
			)
		);

		var ConnectionString = configuration.GetConnectionString("DefaultConnection") ??
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(ConnectionString));


		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<IEmailSender, EmailService>();
		services.AddScoped<IGoogleAuthService, GoogleAuthService>();
		services.AddScoped<IImageService, ImageService>();

		services
			.AddMapsterConfig()
			.AddFluentValidationConfig()
			.AddAuthConfig(configuration)
			.AddRateLimitingConfig();


		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();
		services.AddBackgroundJobsConfig(configuration);

		services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

		services.Configure<GoogleSettings>(configuration.GetSection(nameof(GoogleSettings)));

		services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));

		services.Configure<CloudinarySettings>(configuration.GetSection(nameof(CloudinarySettings)));

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

	private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
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
			options.SignIn.RequireConfirmedEmail = true;
			options.User.RequireUniqueEmail = true;
		});

		return services;
	}

	private static IServiceCollection AddBackgroundJobsConfig(this IServiceCollection services, IConfiguration configuration)
	{
		// Hangfire
		services.AddHangfire(config => config
			.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
			.UseSimpleAssemblyNameTypeSerializer()
			.UseRecommendedSerializerSettings()
			.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection"))
		);

		services.AddHangfireServer();

		services.AddScoped<CleanUpExpiredRefreshTokensJob>();
		services.AddScoped<CleanUpExpiredPasswordResetCodesJob>();


		return services;
	}

	private static IServiceCollection AddRateLimitingConfig(this IServiceCollection services)
	{
		services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			static string GetClientKey(HttpContext context)
			{
				return context.User.Identity?.IsAuthenticated == true
					? context.User.GetUserId()!
					: context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			}

			// 30 requests per minute with queuing
			options.AddPolicy(RateLimiters.GeneralPolicy, context =>
				RateLimitPartition.GetSlidingWindowLimiter(
					GetClientKey(context),
					_ => new SlidingWindowRateLimiterOptions
					{
						PermitLimit = 30,
						Window = TimeSpan.FromMinutes(1),
						SegmentsPerWindow = 6,
						QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
						QueueLimit = 5
					}
				)
			);

			// 10 requests per minute without queuing
			options.AddPolicy(RateLimiters.AuthPolicy, context =>
				RateLimitPartition.GetSlidingWindowLimiter(
					GetClientKey(context),
					_ => new SlidingWindowRateLimiterOptions
					{
						PermitLimit = 10,
						Window = TimeSpan.FromMinutes(1),
						SegmentsPerWindow = 6,
						QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
						QueueLimit = 0
					}
				)
			);

			// 5 requests per minute without queuing
			options.AddPolicy(RateLimiters.SensitivePolicy, context =>
				RateLimitPartition.GetSlidingWindowLimiter(
					GetClientKey(context),
					_ => new SlidingWindowRateLimiterOptions
					{
						PermitLimit = 3,
						Window = TimeSpan.FromMinutes(1),
						SegmentsPerWindow = 6,
						QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
						QueueLimit = 0
					}
				)
			);

			// Custom Response
			options.OnRejected = async (context, cancellationToken) =>
			{
				context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

				if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
					context.HttpContext.Response.Headers.RetryAfter =
						((int)retryAfter.TotalSeconds).ToString();

				await context.HttpContext.Response.WriteAsJsonAsync(new
				{
					type = "https://tools.ietf.org/html/rfc6585#section-4",
					title = "Too Many Requests",
					status = 429,
					detail = "You have exceeded the rate limit. Please try again later."
				}, cancellationToken);
			};
		});

		return services;
	}
}