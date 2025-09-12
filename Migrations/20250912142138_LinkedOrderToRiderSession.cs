using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class LinkedOrderToRiderSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "RiderSessionId1",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RiderSessionId1",
                table: "Orders",
                column: "RiderSessionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders",
                column: "RiderSessionId",
                principalTable: "RiderSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId1",
                table: "Orders",
                column: "RiderSessionId1",
                principalTable: "RiderSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RiderSessionId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RiderSessionId1",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders",
                column: "RiderSessionId",
                principalTable: "RiderSessions",
                principalColumn: "Id");
        }
    }
}
