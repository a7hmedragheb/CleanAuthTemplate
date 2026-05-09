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

		builder
			.OwnsMany(u => u.RefreshTokens)
			.ToTable("RefreshTokens")
			.WithOwner()
			.HasForeignKey("UserId");

		// Global Filter for disappear Deleted Users Query Results
		builder.HasQueryFilter(u => !u.IsDisabled);


		//Define Data
		builder.HasData(new ApplicationUser
		{
			Id = DefaultUsers.Admin.Id,
			FirstName = DefaultUsers.Admin.FirstName,
			LastName = DefaultUsers.Admin.LastName,
			Gender = DefaultUsers.Admin.gender,
			PhoneNumber = DefaultUsers.Admin.PhoneNumber,
			UserName = DefaultUsers.Admin.Email,
			NormalizedUserName = DefaultUsers.Admin.Email.ToUpper(),
			Email = DefaultUsers.Admin.Email,
			NormalizedEmail = DefaultUsers.Admin.Email.ToUpper(),
			SecurityStamp = DefaultUsers.Admin.SecurityStamp,
			ConcurrencyStamp = DefaultUsers.Admin.ConcurrencyStamp,
			EmailConfirmed = true,
			PasswordHash = DefaultUsers.Admin.PasswordHash
		});
	}
}