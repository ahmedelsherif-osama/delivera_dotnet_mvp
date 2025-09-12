using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSessionOrderRelation4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DeleteData(
            //     table: "Users",
            //     keyColumn: "Id",
            //     keyValue: new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.InsertData(
            //     table: "Users",
            //     columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
            //     values: new object[] { new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"), new DateTime(2025, 9, 8, 9, 42, 7, 48, DateTimeKind.Utc).AddTicks(1327), null, new DateTime(2025, 9, 8, 9, 42, 7, 48, DateTimeKind.Utc).AddTicks(1331), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
