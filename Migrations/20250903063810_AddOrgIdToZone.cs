using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgIdToZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("802f2fcf-d0e8-434e-bd09-0e976231324b"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Zones",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("bf43cfe6-e993-4e57-9573-a75ee7bd89bd"), new DateTime(2025, 9, 3, 6, 38, 9, 994, DateTimeKind.Utc).AddTicks(1899), null, new DateTime(2025, 9, 3, 6, 38, 9, 994, DateTimeKind.Utc).AddTicks(1903), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });

            migrationBuilder.CreateIndex(
                name: "IX_Zones_OrganizationId",
                table: "Zones",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Zones_Organizations_OrganizationId",
                table: "Zones",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zones_Organizations_OrganizationId",
                table: "Zones");

            migrationBuilder.DropIndex(
                name: "IX_Zones_OrganizationId",
                table: "Zones");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("bf43cfe6-e993-4e57-9573-a75ee7bd89bd"));

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Zones");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("802f2fcf-d0e8-434e-bd09-0e976231324b"), new DateTime(2025, 9, 3, 5, 45, 20, 294, DateTimeKind.Utc).AddTicks(8532), null, new DateTime(2025, 9, 3, 5, 45, 20, 294, DateTimeKind.Utc).AddTicks(8536), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
