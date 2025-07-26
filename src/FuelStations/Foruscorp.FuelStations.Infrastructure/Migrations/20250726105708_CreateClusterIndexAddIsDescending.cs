using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateClusterIndexAddIsDescending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FuelStations_Latitude",
                schema: "FuelStation",
                table: "FuelStations",
                column: "Latitude");

            migrationBuilder.CreateIndex(
                name: "IX_FuelStations_Longitude",
                schema: "FuelStation",
                table: "FuelStations",
                column: "Longitude");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FuelStations_Latitude",
                schema: "FuelStation",
                table: "FuelStations");

            migrationBuilder.DropIndex(
                name: "IX_FuelStations_Longitude",
                schema: "FuelStation",
                table: "FuelStations");
        }
    }
}
