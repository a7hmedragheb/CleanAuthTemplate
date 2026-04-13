using HangfireBasicAuthenticationFilter;
using Template.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}


// Recurring Jobs
using (var scope = app.Services.CreateScope())
{
	var recurringJobManager = scope.ServiceProvider
		.GetRequiredService<IRecurringJobManager>();

	// every day at 12 AM
	recurringJobManager.AddOrUpdate<CleanUpExpiredRefreshTokensJob>(
		"cleanup-expired-refresh-tokens",
		job => job.ExecuteAsync(),
		Cron.Daily()
	);

	// every Hour
	recurringJobManager.AddOrUpdate<CleanUpExpiredPasswordResetCodesJob>(
		 "cleanup-expired-reset-codes",
		job => job.ExecuteAsync(),
		Cron.Hourly()
	);
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

// https://localhost:7131/jobs
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
	Authorization =
	[
		new HangfireCustomBasicAuthenticationFilter
		{
			User = builder.Configuration.GetValue<string>("HangfireSettings:Username"),
			Pass = builder.Configuration.GetValue<string>("HangfireSettings:Password"),
		}
	],
	DashboardTitle = "Auth Template Service - Job Dashboard",
	IsReadOnlyFunc = (context) => true
});

app.MapControllers();

app.Run();
