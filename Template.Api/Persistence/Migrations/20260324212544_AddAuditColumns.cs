using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdatedOn",
                table: "AspNetUsers",
                newName: "UpdatedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "AspNetUsers",
                newName: "LastUpdatedOn");
        }
    }
}
