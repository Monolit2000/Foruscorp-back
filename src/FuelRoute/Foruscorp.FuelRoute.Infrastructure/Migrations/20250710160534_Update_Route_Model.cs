using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Route_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FuelNeeded",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "RemainingFuel",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FuelNeeded",
                schema: "FuelRoutes",
                table: "RouteSections");

            migrationBuilder.DropColumn(
                name: "RemainingFuel",
                schema: "FuelRoutes",
                table: "FuelRoutes");
        }
    }
}
