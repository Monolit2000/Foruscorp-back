using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForignKay_to_CL_TrucksTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.RenameColumn(
                name: "TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                newName: "CurrentTruckTrackerId");

            migrationBuilder.RenameIndex(
                name: "IX_TruckLocationHistory_TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                newName: "IX_TruckLocationHistory_CurrentTruckTrackerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_CurrentTruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "CurrentTruckTrackerId",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_CurrentTruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.RenameColumn(
                name: "CurrentTruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                newName: "TruckTrackerId1");

            migrationBuilder.RenameIndex(
                name: "IX_TruckLocationHistory_CurrentTruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                newName: "IX_TruckLocationHistory_TruckTrackerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "TruckTrackerId1",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
