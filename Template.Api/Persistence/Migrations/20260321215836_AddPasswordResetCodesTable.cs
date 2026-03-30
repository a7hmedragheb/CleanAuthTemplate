using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Api.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class AddPasswordResetCodesTable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PasswordResetCodes",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					CodeHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
					IdentityToken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
					ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
					Attempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PasswordResetCodes", x => x.Id);
					table.ForeignKey(
						name: "FK_PasswordResetCodes_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_PasswordResetCodes_UserId",
				table: "PasswordResetCodes",
				column: "UserId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PasswordResetCodes");
		}
	}
}
