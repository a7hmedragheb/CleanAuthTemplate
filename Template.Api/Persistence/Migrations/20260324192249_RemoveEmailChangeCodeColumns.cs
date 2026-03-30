using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Api.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class RemoveEmailChangeCodeColumns : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "EmailChangeCodeExpiresAt",
				table: "AspNetUsers");

			migrationBuilder.DropColumn(
				name: "EmailChangeCodeHash",
				table: "AspNetUsers");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTime>(
				name: "EmailChangeCodeExpiresAt",
				table: "AspNetUsers",
				type: "datetime2",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "EmailChangeCodeHash",
				table: "AspNetUsers",
				type: "nvarchar(256)",
				maxLength: 256,
				nullable: true);
		}
	}
}
