using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Udpate_NearFuelStationPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNear",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "NearDistance",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordedOnLocationId",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NearFuelStationPlans_RecordedOnLocationId",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                column: "RecordedOnLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_NearFuelStationPlans_TruckLocationHistory_RecordedOnLocatio~",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                column: "RecordedOnLocationId",
                principalSchema: "TuckTracking",
                principalTable: "TruckLocationHistory",
                principalColumn: "TruckLocationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NearFuelStationPlans_TruckLocationHistory_RecordedOnLocatio~",
                schema: "TuckTracking",
                table: "NearFuelStationPlans");

            migrationBuilder.DropIndex(
                name: "IX_NearFuelStationPlans_RecordedOnLocationId",
                schema: "TuckTracking",
                table: "NearFuelStationPlans");

            migrationBuilder.DropColumn(
                name: "IsNear",
                schema: "TuckTracking",
                table: "NearFuelStationPlans");

            migrationBuilder.DropColumn(
                name: "NearDistance",
                schema: "TuckTracking",
                table: "NearFuelStationPlans");

            migrationBuilder.DropColumn(
                name: "RecordedOnLocationId",
                schema: "TuckTracking",
                table: "NearFuelStationPlans");
        }
    }
}
