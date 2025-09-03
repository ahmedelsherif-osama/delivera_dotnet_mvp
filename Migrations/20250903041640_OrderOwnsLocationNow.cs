using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class OrderOwnsLocationNow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Locations_DropOffLocationId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Locations_PickUpLocationId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DropOffLocationId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PickUpLocationId",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("03d2aaf4-f494-4f17-9b6e-c1040aee1d23"));

            migrationBuilder.RenameColumn(
                name: "PickUpLocationId",
                table: "Orders",
                newName: "PickUpLocation_Timestamp");

            migrationBuilder.RenameColumn(
                name: "DropOffLocationId",
                table: "Orders",
                newName: "PickUpLocation_Id");

            migrationBuilder.AddColumn<string>(
                name: "DropOffLocation_Address",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DropOffLocation_Id",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "DropOffLocation_Latitude",
                table: "Orders",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DropOffLocation_Longitude",
                table: "Orders",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DropOffLocation_Timestamp",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "OrderDetails",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickUpLocation_Address",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PickUpLocation_Latitude",
                table: "Orders",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PickUpLocation_Longitude",
                table: "Orders",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("2a23bb96-edab-4e1b-8523-dfd1c2bd9631"), new DateTime(2025, 9, 3, 4, 16, 39, 653, DateTimeKind.Utc).AddTicks(199), null, new DateTime(2025, 9, 3, 4, 16, 39, 653, DateTimeKind.Utc).AddTicks(205), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("2a23bb96-edab-4e1b-8523-dfd1c2bd9631"));

            migrationBuilder.DropColumn(
                name: "DropOffLocation_Address",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DropOffLocation_Id",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DropOffLocation_Latitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DropOffLocation_Longitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DropOffLocation_Timestamp",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderDetails",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickUpLocation_Address",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickUpLocation_Latitude",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickUpLocation_Longitude",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "PickUpLocation_Timestamp",
                table: "Orders",
                newName: "PickUpLocationId");

            migrationBuilder.RenameColumn(
                name: "PickUpLocation_Id",
                table: "Orders",
                newName: "DropOffLocationId");

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("03d2aaf4-f494-4f17-9b6e-c1040aee1d23"), new DateTime(2025, 9, 2, 6, 38, 34, 550, DateTimeKind.Utc).AddTicks(7967), null, new DateTime(2025, 9, 2, 6, 38, 34, 550, DateTimeKind.Utc).AddTicks(7975), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DropOffLocationId",
                table: "Orders",
                column: "DropOffLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PickUpLocationId",
                table: "Orders",
                column: "PickUpLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Locations_DropOffLocationId",
                table: "Orders",
                column: "DropOffLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Locations_PickUpLocationId",
                table: "Orders",
                column: "PickUpLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
