using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivera.Migrations
{
    /// <inheritdoc />
    public partial class AddRiderNameToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RiderName",
                table: "RiderSessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiderName",
                table: "RiderSessions");
        }
    }
}
