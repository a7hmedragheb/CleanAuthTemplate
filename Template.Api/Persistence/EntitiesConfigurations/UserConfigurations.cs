using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Api.Entities;

namespace Template.Api.Persistence.EntitiesConfigurations;

public class UserConfigurations : IEntityTypeConfiguration<ApplicationUser>
{
	public void Configure(EntityTypeBuilder<ApplicationUser> builder)
	{
		builder.Property(u => u.FirstName)
			.HasMaxLength(250)
			.IsRequired();

		builder.Property(u => u.LastName)
			.HasMaxLength(250)
			.IsRequired();

		builder.Property(u => u.DateOfBirth)
			.IsRequired();

		builder.Property(u => u.Gender)
			.IsRequired();
	}
}