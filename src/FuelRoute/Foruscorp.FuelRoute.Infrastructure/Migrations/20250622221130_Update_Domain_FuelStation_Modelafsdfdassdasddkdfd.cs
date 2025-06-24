using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Domain_FuelStation_Modelafsdfdassdasddkdfd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "FuelRoutes");

            migrationBuilder.CreateTable(
                name: "FuelRoutes",
                schema: "FuelRoutes",
                columns: table => new
                {
                    FuelRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelRoutes", x => x.FuelRouteId);
                });

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

            migrationBuilder.CreateTable(
                name: "MapPoints",
                schema: "FuelRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapPoints_FuelRoutes_RouteId",
                        column: x => x.RouteId,
                        principalSchema: "FuelRoutes",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteSections",
                schema: "FuelRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncodeRoute = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteSections_FuelRoutes_RouteId",
                        column: x => x.RouteId,
                        principalSchema: "FuelRoutes",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteFuelPoints",
                schema: "FuelRoutes",
                columns: table => new
                {
                    FuelStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PriceAfterDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Latitude = table.Column<string>(type: "text", nullable: true),
                    Longitude = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    IsAlgorithm = table.Column<bool>(type: "boolean", nullable: false),
                    Refill = table.Column<string>(type: "text", nullable: true),
                    StopOrder = table.Column<int>(type: "integer", nullable: false),
                    NextDistanceKm = table.Column<string>(type: "text", nullable: true),
                    RoadSectionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteFuelPoints", x => x.FuelStationId);
                    table.ForeignKey(
                        name: "FK_RouteFuelPoints_FuelRoutes_FuelRouteId",
                        column: x => x.FuelRouteId,
                        principalSchema: "FuelRoutes",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteFuelPoints_RouteSections_RoadSectionId",
                        column: x => x.RoadSectionId,
                        principalSchema: "FuelRoutes",
                        principalTable: "RouteSections",
                        principalColumn: "Id",
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
                name: "IX_FuelRoutes_TruckId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationPoints_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_MapPoints_RouteId",
                schema: "FuelRoutes",
                table: "MapPoints",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteFuelPoints_FuelRouteId",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                column: "FuelRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteFuelPoints_RoadSectionId",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                column: "RoadSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSections_RouteId",
                schema: "FuelRoutes",
                table: "RouteSections",
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
                name: "MapPoints",
                schema: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "RouteFuelPoints",
                schema: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "RouteSections",
                schema: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "LocationPoints",
                schema: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "FuelRoutes",
                schema: "FuelRoutes");
        }
    }
}
