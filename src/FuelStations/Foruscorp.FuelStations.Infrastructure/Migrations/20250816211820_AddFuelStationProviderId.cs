using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelStationProviderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FuelStationProviderId",
                schema: "FuelStation",
                table: "FuelStations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelStations_FuelStationProviderId",
                schema: "FuelStation",
                table: "FuelStations",
                column: "FuelStationProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FuelStations_FuelStationProviderId",
                schema: "FuelStation",
                table: "FuelStations");

            migrationBuilder.DropColumn(
                name: "FuelStationProviderId",
                schema: "FuelStation",
                table: "FuelStations");
        }
    }
}
