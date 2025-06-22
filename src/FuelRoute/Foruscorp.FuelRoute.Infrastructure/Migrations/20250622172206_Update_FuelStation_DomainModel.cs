using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_FuelStation_DomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionId",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.RenameColumn(
                name: "GeoPoint",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                newName: "Longitude");

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Longitude",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAlgorithm",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NextDistanceKm",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Refill",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoadSectionId",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StopOrder",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlgorithm",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "NextDistanceKm",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "Refill",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "RoadSectionId",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "StopOrder",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                newName: "GeoPoint");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<double>(
                name: "GeoPoint",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "SectionId",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: true);
        }
    }
}
