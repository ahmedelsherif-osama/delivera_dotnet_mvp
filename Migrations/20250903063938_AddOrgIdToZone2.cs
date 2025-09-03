using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgIdToZone2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bf43cfe6-e993-4e57-9573-a75ee7bd89bd"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("5e593af0-61ba-4052-821e-9c23e11909fa"), new DateTime(2025, 9, 3, 6, 39, 37, 979, DateTimeKind.Utc).AddTicks(3325), null, new DateTime(2025, 9, 3, 6, 39, 37, 979, DateTimeKind.Utc).AddTicks(3332), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5e593af0-61ba-4052-821e-9c23e11909fa"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("bf43cfe6-e993-4e57-9573-a75ee7bd89bd"), new DateTime(2025, 9, 3, 6, 38, 9, 994, DateTimeKind.Utc).AddTicks(1899), null, new DateTime(2025, 9, 3, 6, 38, 9, 994, DateTimeKind.Utc).AddTicks(1903), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
