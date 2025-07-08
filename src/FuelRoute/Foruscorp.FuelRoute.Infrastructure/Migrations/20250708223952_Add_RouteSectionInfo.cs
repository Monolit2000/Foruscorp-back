using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RouteSectionInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriveTime",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Gallons",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Miles",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Tolls",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriveTime",
                schema: "FuelRoutes",
                table: "RouteSections");

            migrationBuilder.DropColumn(
                name: "Gallons",
                schema: "FuelRoutes",
                table: "RouteSections");

            migrationBuilder.DropColumn(
                name: "Miles",
                schema: "FuelRoutes",
                table: "RouteSections");

            migrationBuilder.DropColumn(
                name: "Tolls",
                schema: "FuelRoutes",
                table: "RouteSections");
        }
    }
}
