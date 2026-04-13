namespace Template.Api.Jobs;

public class CleanUpExpiredRefreshTokensJob(
	UserManager<ApplicationUser> userManager,
	ILogger<CleanUpExpiredRefreshTokensJob> logger)
{
	[AutomaticRetry(Attempts = 3)]
	public async Task ExecuteAsync()
	{
		logger.LogInformation("Starting cleanup of expired refresh tokens...");

		var users = await userManager.Users
			.Where(u => u.RefreshTokens.Any(t => t.RevokedOn != null || t.ExpiresOn <= DateTime.UtcNow))
			.Include(u => u.RefreshTokens)
			.ToListAsync();

		var totalRemoved = 0;

		foreach (var user in users)
		{
			var expiredTokens = user.RefreshTokens
				.Where(t => t.RevokedOn != null || t.ExpiresOn <= DateTime.UtcNow)
				.ToList();

			foreach (var token in expiredTokens)
				user.RefreshTokens.Remove(token);

			if (expiredTokens.Count > 0)
			{
				await userManager.UpdateAsync(user);
				totalRemoved += expiredTokens.Count;
			}
		}

		logger.LogInformation("Removed {Count} expired refresh tokens", totalRemoved);
	}
}