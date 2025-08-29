using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Forward_Add_Order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "NextDistanceKm",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "StopOrder",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextDistanceKm",
                schema: "FuelRoutes",
                table: "FuelStationChanges");

            migrationBuilder.DropColumn(
                name: "StopOrder",
                schema: "FuelRoutes",
                table: "FuelStationChanges");
        }
    }
}
