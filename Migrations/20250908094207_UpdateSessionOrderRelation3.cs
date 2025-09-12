using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSessionOrderRelation3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("837cca0e-438f-4fe5-ad8a-4eef74ed9058"));

            // migrationBuilder.InsertData(
            //     table: "Users",
            //     columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
            //     values: new object[] { new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"), new DateTime(2025, 9, 8, 9, 42, 7, 48, DateTimeKind.Utc).AddTicks(1327), null, new DateTime(2025, 9, 8, 9, 42, 7, 48, DateTimeKind.Utc).AddTicks(1331), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DeleteData(
            //     table: "Users",
            //     keyColumn: "Id",
            //     keyValue: new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("837cca0e-438f-4fe5-ad8a-4eef74ed9058"), new DateTime(2025, 9, 8, 9, 34, 13, 944, DateTimeKind.Utc).AddTicks(1235), null, new DateTime(2025, 9, 8, 9, 34, 13, 944, DateTimeKind.Utc).AddTicks(1239), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
