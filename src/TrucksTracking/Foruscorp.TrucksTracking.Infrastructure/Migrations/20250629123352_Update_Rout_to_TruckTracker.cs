using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Rout_to_TruckTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TruckTrackers_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.CreateIndex(
                name: "IX_TruckTrackers_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentRouteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TruckTrackers_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.CreateIndex(
                name: "IX_TruckTrackers_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentRouteId");
        }
    }
}
