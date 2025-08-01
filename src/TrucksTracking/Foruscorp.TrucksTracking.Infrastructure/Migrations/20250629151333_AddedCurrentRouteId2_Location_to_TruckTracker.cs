﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCurrentRouteId2_Location_to_TruckTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckLocationHistory_Routes_RouteId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropIndex(
                name: "IX_TruckLocationHistory_RouteId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TruckLocationHistory_RouteId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckLocationHistory_Routes_RouteId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "RouteId",
                principalSchema: "TuckTracking",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
