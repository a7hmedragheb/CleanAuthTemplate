using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

		builder
			.OwnsMany(u => u.RefreshTokens)
			.ToTable("RefreshTokens")
			.WithOwner()
			.HasForeignKey("UserId");

		builder.Property(u => u.PendingEmail)
			.HasMaxLength(256);

		builder.Property(u => u.EmailChangeCodeHash)
			.HasMaxLength(256);

		builder.Property(u => u.EmailChangeCodeExpiresAt);

		builder.Property(u => u.IsDeleted)
			.IsRequired()
			.HasDefaultValue(false);

		builder.Property(u => u.DeletedAt);

		// Global Filter for disappear Deleted Users Query Results
		builder.HasQueryFilter(u => !u.IsDeleted);
	}
}