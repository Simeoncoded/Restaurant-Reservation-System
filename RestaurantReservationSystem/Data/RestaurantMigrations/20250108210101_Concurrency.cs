using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantReservationSystem.Data.RestaurantMigrations
{
    /// <inheritdoc />
    public partial class Concurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tables",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Reservations",
                type: "BLOB",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Reservations");
        }
    }
}
