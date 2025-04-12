using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class innit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "FuelRoute");

            migrationBuilder.CreateTable(
                name: "FuelRoutes",
                schema: "FuelRoute",
                columns: table => new
                {
                    FuelRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    Origin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Destination = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelRoutes", x => x.FuelRouteId);
                });

            migrationBuilder.CreateTable(
                name: "RouteFuelPoints",
                schema: "FuelRoute",
                columns: table => new
                {
                    FuelPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    GeoPoint = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    FuelPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteFuelPoints", x => x.FuelPointId);
                    table.ForeignKey(
                        name: "FK_RouteFuelPoints_FuelRoutes_FuelRouteId",
                        column: x => x.FuelRouteId,
                        principalSchema: "FuelRoute",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelRoutes_TruckId",
                schema: "FuelRoute",
                table: "FuelRoutes",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteFuelPoints_FuelRouteId",
                schema: "FuelRoute",
                table: "RouteFuelPoints",
                column: "FuelRouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteFuelPoints",
                schema: "FuelRoute");

            migrationBuilder.DropTable(
                name: "FuelRoutes",
                schema: "FuelRoute");
        }
    }
}
