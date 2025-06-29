using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCurrentRouteId3_Location_to_TruckTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TruckEngineStates",
                schema: "TuckTracking",
                table: "TruckTrackers",
                newName: "TruckEngineStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TruckEngineStatus",
                schema: "TuckTracking",
                table: "TruckTrackers",
                newName: "TruckEngineStates");
        }
    }
}
