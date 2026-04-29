using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Template.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "019dd94b-9c35-78a3-b7e1-90544b8be79e", "019dd94b-9c35-78a3-b7e1-90552fb9ab80", false, false, "Admin", "ADMIN" },
                    { "019dd94b-9c35-78a3-b7e1-90560e296a0b", "019dd94b-9c35-78a3-b7e1-90575fd7c247", true, false, "Member", "MEMBER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DateOfBirth", "DeletedAt", "Email", "EmailConfirmed", "FirstName", "Gender", "ImagePublicId", "ImageThumbnailUrl", "ImageUrl", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PendingEmail", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "019dd94b-9c35-78a3-b7e1-905263792220", 0, "019dd94b-9c35-78a3-b7e1-9053bc21473b", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@template.com", true, "Template", null, null, null, null, "Admin", false, null, "ADMIN@TEMPLATE.COM", "ADMIN@TEMPLATE.COM", "AQAAAAIAAYagAAAAEEb4m4HWcwdNhyuLoVUfe255hAp2E8U9fa2QzfIHzOUYTI4khgryMuyWVsHcOslRVA==", null, null, false, "045A9DEA16F5473DA2C73C6D788FEACE", false, "admin@template.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "019dd94b-9c35-78a3-b7e1-90544b8be79e", "019dd94b-9c35-78a3-b7e1-905263792220" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019dd94b-9c35-78a3-b7e1-90560e296a0b");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "019dd94b-9c35-78a3-b7e1-90544b8be79e", "019dd94b-9c35-78a3-b7e1-905263792220" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019dd94b-9c35-78a3-b7e1-90544b8be79e");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019dd94b-9c35-78a3-b7e1-905263792220");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
