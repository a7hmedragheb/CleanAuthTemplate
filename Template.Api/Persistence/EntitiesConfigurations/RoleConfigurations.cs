using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Template.Api.Persistence.EntitiesConfigurations;

public class RoleConfigurations : IEntityTypeConfiguration<ApplicationRole>
{
	public void Configure(EntityTypeBuilder<ApplicationRole> builder)
	{
		//Define Data
		builder.HasData(
		[
			new ApplicationRole
			{
				Id = DefaultRoles.Admin.Id,
				Name = DefaultRoles.Admin.Name,
				NormalizedName = DefaultRoles.Admin.Name.ToUpper(),
				ConcurrencyStamp = DefaultRoles.Admin.ConcurrencyStamp
			},
			new ApplicationRole
			{
				Id = DefaultRoles.Member.Id,
				Name = DefaultRoles.Member.Name,
				NormalizedName = DefaultRoles.Member.Name.ToUpper(),
				ConcurrencyStamp = DefaultRoles.Member.ConcurrencyStamp,
				IsDefault = true
			}
		]);
	}
}