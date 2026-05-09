using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Template.Api.Persistence.EntitiesConfigurations;

public class UserRoleConfigurations : IEntityTypeConfiguration<IdentityUserRole<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
	{
		//Define Data
		builder.HasData(new IdentityUserRole<string>
			{
				UserId = DefaultUsers.Admin.Id,
				RoleId = DefaultRoles.Admin.Id
			}
		);
	}
}