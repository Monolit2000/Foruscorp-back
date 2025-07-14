using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCurrentTruchLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_CurrentTruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropIndex(
                name: "IX_TruckLocationHistory_CurrentTruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentTruckLocationId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TruckTrackers_CurrentTruckLocationId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentTruckLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckTrackers_TruckLocationHistory_CurrentTruckLocationId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentTruckLocationId",
                principalSchema: "TuckTracking",
                principalTable: "TruckLocationHistory",
                principalColumn: "TruckLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckTrackers_TruckLocationHistory_CurrentTruckLocationId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.DropIndex(
                name: "IX_TruckTrackers_CurrentTruckLocationId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.DropColumn(
                name: "CurrentTruckLocationId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLocationHistory_CurrentTruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "CurrentTruckTrackerId",
                unique: true);

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
    }
}
