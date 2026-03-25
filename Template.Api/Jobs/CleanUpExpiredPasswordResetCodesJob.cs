using Hangfire;

namespace Template.Api.Jobs;
public class CleanUpExpiredPasswordResetCodesJob(
	ApplicationDbContext context,
	ILogger<CleanUpExpiredPasswordResetCodesJob> logger)
{
	[AutomaticRetry(Attempts = 3)]
	public async Task ExecuteAsync()
	{
		logger.LogInformation("Starting cleanup of expired password reset codes...");

		var deleted = await context.PasswordResetCodes
			.Where(x => x.ExpiresAt < DateTime.UtcNow || x.UsedAt != null)
			.ExecuteDeleteAsync();

		logger.LogInformation("Removed {Count} expired password reset codes", deleted);
	}
}