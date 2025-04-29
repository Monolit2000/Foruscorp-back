using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddeProviderNameToFuelStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                schema: "FuelStation",
                table: "FuelStations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderName",
                schema: "FuelStation",
                table: "FuelStations");
        }
    }
}
