namespace Template.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
	{
		var ConnectionString = configuration.GetConnectionString("DefaultConnection") ??
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(ConnectionString));

		return services;
	}
}