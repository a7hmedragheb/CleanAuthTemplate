using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Template.Api.Persistence;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		base.OnModelCreating(modelBuilder);
	}
}