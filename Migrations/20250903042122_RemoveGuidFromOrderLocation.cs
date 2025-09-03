using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGuidFromOrderLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("2a23bb96-edab-4e1b-8523-dfd1c2bd9631"));

            migrationBuilder.DropColumn(
                name: "DropOffLocation_Id",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickUpLocation_Id",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("16108c4e-1c11-4139-af4d-ef256341b0a7"), new DateTime(2025, 9, 3, 4, 21, 21, 818, DateTimeKind.Utc).AddTicks(5363), null, new DateTime(2025, 9, 3, 4, 21, 21, 818, DateTimeKind.Utc).AddTicks(5366), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("16108c4e-1c11-4139-af4d-ef256341b0a7"));

            migrationBuilder.AddColumn<Guid>(
                name: "DropOffLocation_Id",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PickUpLocation_Id",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("2a23bb96-edab-4e1b-8523-dfd1c2bd9631"), new DateTime(2025, 9, 3, 4, 16, 39, 653, DateTimeKind.Utc).AddTicks(199), null, new DateTime(2025, 9, 3, 4, 16, 39, 653, DateTimeKind.Utc).AddTicks(205), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
