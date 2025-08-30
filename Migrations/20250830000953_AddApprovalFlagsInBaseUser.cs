using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFlagsInBaseUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3a53b18e-c4ec-4377-8b9a-3d71b17ac632"));

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "IsSuperAdminApproved");

            migrationBuilder.AddColumn<bool>(
                name: "IsOrgOwnerApproved",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("886c8ebb-d966-4d13-a21c-2ff52212e7ce"), null, new DateTime(2025, 8, 30, 0, 9, 52, 771, DateTimeKind.Utc).AddTicks(7329), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("886c8ebb-d966-4d13-a21c-2ff52212e7ce"));

            migrationBuilder.DropColumn(
                name: "IsOrgOwnerApproved",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "IsSuperAdminApproved",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsActive", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("3a53b18e-c4ec-4377-8b9a-3d71b17ac632"), null, new DateTime(2025, 8, 29, 21, 59, 4, 665, DateTimeKind.Utc).AddTicks(751), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
