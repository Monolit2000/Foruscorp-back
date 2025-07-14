using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_NearFuelStationPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NearFuelStationPlans",
                schema: "TuckTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NearFuelStationPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NearFuelStationPlans_FuelStationId",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                column: "FuelStationId");

            migrationBuilder.CreateIndex(
                name: "IX_NearFuelStationPlans_TruckId",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                column: "TruckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NearFuelStationPlans",
                schema: "TuckTracking");
        }
    }
}
