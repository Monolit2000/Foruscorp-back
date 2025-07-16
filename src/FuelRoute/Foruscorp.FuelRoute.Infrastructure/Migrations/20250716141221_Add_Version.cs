using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Version : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOld",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "LocationPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentVersion",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCountRoutVersions",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "RouteSections");

            migrationBuilder.DropColumn(
                name: "IsOld",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.DropColumn(
                name: "CurrentVersion",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropColumn(
                name: "RouteVersion",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropColumn(
                name: "TotalCountRoutVersions",
                schema: "FuelRoutes",
                table: "FuelRoutes");
        }
    }
}
