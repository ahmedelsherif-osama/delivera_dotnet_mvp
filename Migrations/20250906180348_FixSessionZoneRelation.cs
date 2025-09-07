using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class FixSessionZoneRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3b5afa9d-388e-4548-960a-7d0c73f4ea92"));

            migrationBuilder.DropColumn(
                name: "Zone",
                table: "RiderSessions");

            migrationBuilder.AddColumn<Guid>(
                name: "ZoneId",
                table: "RiderSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"), new DateTime(2025, 9, 6, 18, 3, 48, 57, DateTimeKind.Utc).AddTicks(2485), null, new DateTime(2025, 9, 6, 18, 3, 48, 57, DateTimeKind.Utc).AddTicks(2490), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });

            migrationBuilder.CreateIndex(
                name: "IX_RiderSessions_ZoneId",
                table: "RiderSessions",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_RiderSessions_Zones_ZoneId",
                table: "RiderSessions",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiderSessions_Zones_ZoneId",
                table: "RiderSessions");

            migrationBuilder.DropIndex(
                name: "IX_RiderSessions_ZoneId",
                table: "RiderSessions");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d87d1db6-9321-4323-9af1-6157f85d2744"));

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "RiderSessions");

            migrationBuilder.AddColumn<string>(
                name: "Zone",
                table: "RiderSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("3b5afa9d-388e-4548-960a-7d0c73f4ea92"), new DateTime(2025, 9, 6, 12, 25, 29, 499, DateTimeKind.Utc).AddTicks(8984), null, new DateTime(2025, 9, 6, 12, 25, 29, 499, DateTimeKind.Utc).AddTicks(8988), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
