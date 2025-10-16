using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class FixLocationSessionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "CurrentOrderDropOff_Address",
                table: "RiderSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentOrderDropOff_Latitude",
                table: "RiderSessions",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentOrderDropOff_Longitude",
                table: "RiderSessions",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentOrderDropOff_Timestamp",
                table: "RiderSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentOrderId",
                table: "RiderSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentOrderPickUp_Address",
                table: "RiderSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentOrderPickUp_Latitude",
                table: "RiderSessions",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentOrderPickUp_Longitude",
                table: "RiderSessions",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentOrderPickUp_Timestamp",
                table: "RiderSessions",
                type: "TEXT",
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "CurrentOrderDropOff_Address",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderDropOff_Latitude",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderDropOff_Longitude",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderDropOff_Timestamp",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderId",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderPickUp_Address",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderPickUp_Latitude",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderPickUp_Longitude",
                table: "RiderSessions");

            migrationBuilder.DropColumn(
                name: "CurrentOrderPickUp_Timestamp",
                table: "RiderSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RiderSessions_RiderSessionId",
                table: "Orders",
                column: "RiderSessionId",
                principalTable: "RiderSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
