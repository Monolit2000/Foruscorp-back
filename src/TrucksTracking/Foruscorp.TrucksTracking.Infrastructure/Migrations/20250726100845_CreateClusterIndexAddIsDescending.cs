﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateClusterIndexAddIsDescending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TruckLocationHistory_TruckId_RecordedAt",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLocationHistory_TruckId_RecordedAt",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                columns: new[] { "TruckId", "RecordedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TruckLocationHistory_TruckId_RecordedAt",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLocationHistory_TruckId_RecordedAt",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                columns: new[] { "TruckId", "RecordedAt" });
        }
    }
}
