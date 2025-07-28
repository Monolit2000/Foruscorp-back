using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_GeoPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FuelStations_Latitude",
                schema: "FuelStation",
                table: "FuelStations");

            migrationBuilder.DropIndex(
                name: "IX_FuelStations_Longitude",
                schema: "FuelStation",
                table: "FuelStations");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "FuelStation",
                table: "FuelStations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "FuelStation",
                table: "FuelStations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Point>(
                name: "Coordinates",
                schema: "FuelStation",
                table: "FuelStations",
                type: "geography(Point, 4326)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coordinates",
                schema: "FuelStation",
                table: "FuelStations");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "FuelStation",
                table: "FuelStations",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "FuelStation",
                table: "FuelStations",
                type: "double precision",
                nullable: true);

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
    }
}
