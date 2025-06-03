using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocationPoint_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocationPoints",
                schema: "FuelRoutes",
                columns: table => new
                {
                    LocationPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationPoints", x => x.LocationPointId);
                    table.ForeignKey(
                        name: "FK_LocationPoints_FuelRoutes_RouteId",
                        column: x => x.RouteId,
                        principalSchema: "FuelRoutes",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelRoutes_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelRoutes_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "OriginLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationPoints_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelRoutes_LocationPoints_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "DestinationLocationId",
                principalSchema: "FuelRoutes",
                principalTable: "LocationPoints",
                principalColumn: "LocationPointId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelRoutes_LocationPoints_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "OriginLocationId",
                principalSchema: "FuelRoutes",
                principalTable: "LocationPoints",
                principalColumn: "LocationPointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuelRoutes_LocationPoints_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropForeignKey(
                name: "FK_FuelRoutes_LocationPoints_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "LocationPoints",
                schema: "FuelRoutes");

            migrationBuilder.DropIndex(
                name: "IX_FuelRoutes_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropIndex(
                name: "IX_FuelRoutes_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropColumn(
                name: "DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropColumn(
                name: "OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");
        }
    }
}
