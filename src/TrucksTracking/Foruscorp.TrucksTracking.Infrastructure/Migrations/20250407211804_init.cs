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
                name: "TruckTrackers",
                schema: "TuckTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FuelStatus = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckTrackers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrentTruckLocations",
                schema: "TuckTracking",
                columns: table => new
                {
                    TruckTrackerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentTruckLocations", x => x.TruckTrackerId);
                    table.ForeignKey(
                        name: "FK_CurrentTruckLocations_TruckTrackers_TruckTrackerId",
                        column: x => x.TruckTrackerId,
                        principalSchema: "TuckTracking",
                        principalTable: "TruckTrackers",
                        principalColumn: "Id",
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
                        name: "FK_TruckFuelHistory_TruckTrackers_TruckId",
                        column: x => x.TruckId,
                        principalSchema: "TuckTracking",
                        principalTable: "TruckTrackers",
                        principalColumn: "Id",
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
                        name: "FK_TruckLocationHistory_TruckTrackers_TruckId",
                        column: x => x.TruckId,
                        principalSchema: "TuckTracking",
                        principalTable: "TruckTrackers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TruckTrackers_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckTrackers_TruckId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "TruckId");
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
                name: "TruckTrackers",
                schema: "TuckTracking");
        }
    }
}
