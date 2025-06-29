using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCurrentRouteId_Location_to_TruckTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_TruckTrackers_TruckTrackerId1",
                schema: "TuckTracking",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_TruckTrackerId1",
                schema: "TuckTracking",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "TruckTrackerId1",
                schema: "TuckTracking",
                table: "Routes");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TruckTrackers_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentRouteId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TruckTrackers_Routes_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentRouteId",
                principalSchema: "TuckTracking",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckTrackers_Routes_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.DropIndex(
                name: "IX_TruckTrackers_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.DropColumn(
                name: "CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.AddColumn<Guid>(
                name: "TruckTrackerId1",
                schema: "TuckTracking",
                table: "Routes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_TruckTrackerId1",
                schema: "TuckTracking",
                table: "Routes",
                column: "TruckTrackerId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_TruckTrackers_TruckTrackerId1",
                schema: "TuckTracking",
                table: "Routes",
                column: "TruckTrackerId1",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
