using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
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
                    Origin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Destination = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelRoutes", x => x.FuelRouteId);
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
                name: "RouteFuelPoints",
                schema: "FuelRoutes",
                columns: table => new
                {
                    FuelPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    GeoPoint = table.Column<double>(type: "double precision", nullable: true),
                    FuelPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteFuelPoints", x => x.FuelPointId);
                    table.ForeignKey(
                        name: "FK_RouteFuelPoints_FuelRoutes_FuelRouteId",
                        column: x => x.FuelRouteId,
                        principalSchema: "FuelRoutes",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelRoutes_TruckId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "TruckId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapPoints",
                schema: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "RouteFuelPoints",
                schema: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "FuelRoutes",
                schema: "FuelRoutes");
        }
    }
}
