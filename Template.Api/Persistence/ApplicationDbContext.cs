using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace Template.Api.Persistence;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) :
	IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
	public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

	private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		var cascadeFKs = modelBuilder.Model
		.GetEntityTypes()
		.SelectMany(t => t.GetForeignKeys())
		.Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

		foreach (var fk in cascadeFKs)
			fk.DeleteBehavior = DeleteBehavior.Restrict;

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

		return base.SaveChangesAsync(cancellationToken);
	}
}