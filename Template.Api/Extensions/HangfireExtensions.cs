namespace Template.Api.Extensions;

public static class HangfireExtensions
{
	public static void UseRecurringJobs(this WebApplication app)
	{
		using var scope = app.Services.CreateScope();
		var recurringJobManager = scope.ServiceProvider
			.GetRequiredService<IRecurringJobManager>();

		recurringJobManager.AddOrUpdate<CleanUpExpiredRefreshTokensJob>(
			"cleanup-expired-refresh-tokens",
			job => job.ExecuteAsync(),
			Cron.Daily()
		);

		recurringJobManager.AddOrUpdate<CleanUpExpiredPasswordResetCodesJob>(
			"cleanup-expired-reset-codes",
			job => job.ExecuteAsync(),
			Cron.Daily()
		);
	}
}