using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TuckTracking");

            migrationBuilder.CreateTable(
                name: "Trucks",
                schema: "TuckTracking",
                columns: table => new
                {
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelStatus = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.TruckId);
                });

            migrationBuilder.CreateTable(
                name: "CurrentTruckLocations",
                schema: "TuckTracking",
                columns: table => new
                {
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentTruckLocations", x => x.TruckId);
                    table.ForeignKey(
                        name: "FK_CurrentTruckLocations_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalSchema: "TuckTracking",
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TruckFuelHistory",
                schema: "TuckTracking",
                columns: table => new
                {
                    TruckFuelId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousFuelLevel = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NewFuelLevel = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckFuelHistory", x => new { x.TruckId, x.TruckFuelId });
                    table.ForeignKey(
                        name: "FK_TruckFuelHistory_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalSchema: "TuckTracking",
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TruckLocationHistory",
                schema: "TuckTracking",
                columns: table => new
                {
                    TruckLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckLocationHistory", x => new { x.TruckId, x.TruckLocationId });
                    table.ForeignKey(
                        name: "FK_TruckLocationHistory_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalSchema: "TuckTracking",
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_CurrentRouteId",
                schema: "TuckTracking",
                table: "Trucks",
                column: "CurrentRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_TruckId1",
                schema: "TuckTracking",
                table: "Trucks",
                column: "TruckId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentTruckLocations",
                schema: "TuckTracking");

            migrationBuilder.DropTable(
                name: "TruckFuelHistory",
                schema: "TuckTracking");

            migrationBuilder.DropTable(
                name: "TruckLocationHistory",
                schema: "TuckTracking");

            migrationBuilder.DropTable(
                name: "Trucks",
                schema: "TuckTracking");
        }
    }
}
