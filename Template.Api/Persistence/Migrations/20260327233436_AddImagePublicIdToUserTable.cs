using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePublicIdToUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "AspNetUsers");
        }
    }
}
