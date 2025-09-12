using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSessionOrderRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DeleteData(
            //     table: "Users",
            //     keyColumn: "Id",
            //     keyValue: new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"));

            migrationBuilder.DropColumn(
                name: "ActiveOrders",
                table: "RiderSessions");

            migrationBuilder.AddColumn<Guid>(
                name: "RiderSessionId",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            // migrationBuilder.InsertData(
            //     table: "Users",
            //     columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
            //     values: new object[] { new Guid("f24158bc-3bc2-4efe-9612-b34595b56fba"), new DateTime(2025, 9, 8, 9, 30, 31, 698, DateTimeKind.Utc).AddTicks(4677), null, new DateTime(2025, 9, 8, 9, 30, 31, 698, DateTimeKind.Utc).AddTicks(4681), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RiderSessionId",
                table: "Orders",
                column: "RiderSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders",
                column: "RiderSessionId",
                principalTable: "RiderSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RiderSessionId",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f24158bc-3bc2-4efe-9612-b34595b56fba"));

            migrationBuilder.DropColumn(
                name: "RiderSessionId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "ActiveOrders",
                table: "RiderSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // migrationBuilder.InsertData(
            //     table: "Users",
            //     columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
            //     values: new object[] { new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"), new DateTime(2025, 9, 6, 18, 3, 48, 57, DateTimeKind.Utc).AddTicks(2485), null, new DateTime(2025, 9, 6, 18, 3, 48, 57, DateTimeKind.Utc).AddTicks(2490), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
