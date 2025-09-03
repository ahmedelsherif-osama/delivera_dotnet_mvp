using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaToZoneTableModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("fee2fe56-c96c-4c9e-b40c-5fe8f51cfc87"));

            migrationBuilder.AddColumn<string>(
                name: "WktPolygon",
                table: "Zones",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("802f2fcf-d0e8-434e-bd09-0e976231324b"), new DateTime(2025, 9, 3, 5, 45, 20, 294, DateTimeKind.Utc).AddTicks(8532), null, new DateTime(2025, 9, 3, 5, 45, 20, 294, DateTimeKind.Utc).AddTicks(8536), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("802f2fcf-d0e8-434e-bd09-0e976231324b"));

            migrationBuilder.DropColumn(
                name: "WktPolygon",
                table: "Zones");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("fee2fe56-c96c-4c9e-b40c-5fe8f51cfc87"), new DateTime(2025, 9, 3, 5, 25, 30, 383, DateTimeKind.Utc).AddTicks(1292), null, new DateTime(2025, 9, 3, 5, 25, 30, 383, DateTimeKind.Utc).AddTicks(1297), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
