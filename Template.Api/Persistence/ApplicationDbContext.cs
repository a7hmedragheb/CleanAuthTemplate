using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace Template.Api.Persistence;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) :
	IdentityDbContext<ApplicationUser>(options)
{
	public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

	private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		base.OnModelCreating(modelBuilder);
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

		// AuditableEntity
		var entries = ChangeTracker.Entries<AuditableEntity>();
		foreach (var entityEntry in entries)
		{
			if (entityEntry.State == EntityState.Added)
			{
				entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId!;
				entityEntry.Property(x => x.CreatedOn).CurrentValue = DateTime.UtcNow;
			}
			else if (entityEntry.State == EntityState.Modified)
			{
				entityEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
				entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
			}
		}

		// ApplicationUser
		var userEntries = ChangeTracker.Entries<ApplicationUser>();
		foreach (var entityEntry in userEntries)
		{
			if (entityEntry.State == EntityState.Added)
				entityEntry.Property(x => x.CreatedOn).CurrentValue = DateTime.UtcNow;

			else if (entityEntry.State == EntityState.Modified)
				entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
		}

		return base.SaveChangesAsync(cancellationToken);
	}
}