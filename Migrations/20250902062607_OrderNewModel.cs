using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class OrderNewModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("770ba99e-9c45-4dd7-aa7f-dbcad5865123"));

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "DropOffLocationId",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PickUpLocationId",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("5802bf30-604c-4578-ba84-193669b0de09"), new DateTime(2025, 9, 2, 6, 26, 6, 981, DateTimeKind.Utc).AddTicks(7271), null, new DateTime(2025, 9, 2, 6, 26, 6, 981, DateTimeKind.Utc).AddTicks(7275), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Locations_DropOffLocationId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Locations_PickUpLocationId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DropOffLocationId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PickUpLocationId",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5802bf30-604c-4578-ba84-193669b0de09"));

            migrationBuilder.DropColumn(
                name: "DropOffLocationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickUpLocationId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApprovedAt", "ApprovedById", "CreatedAt", "CreatedById", "DateOfBirth", "Discriminator", "Email", "FirstName", "GlobalRole", "IsOrgOwnerApproved", "IsSuperAdminApproved", "LastName", "NationalId", "OrganizationId", "OrganizationRole", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[] { new Guid("770ba99e-9c45-4dd7-aa7f-dbcad5865123"), new DateTime(2025, 8, 30, 0, 17, 15, 569, DateTimeKind.Utc).AddTicks(1826), null, new DateTime(2025, 8, 30, 0, 17, 15, 569, DateTimeKind.Utc).AddTicks(1830), null, null, "BaseUser", "superadmin@delivera.com", "System", 0, true, true, "Admin", "", null, null, "mkqr8OXPccrizqZGYTzn4qWRn6dY5WgZcEviWjosHws=", "", "superadmin" });
        }
    }
}
