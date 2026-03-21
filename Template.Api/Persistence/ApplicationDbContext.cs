using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using Template.Api.Entities;

namespace Template.Api.Persistence;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
	IdentityDbContext<ApplicationUser>(options)
{

	public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		base.OnModelCreating(modelBuilder);
	}
}