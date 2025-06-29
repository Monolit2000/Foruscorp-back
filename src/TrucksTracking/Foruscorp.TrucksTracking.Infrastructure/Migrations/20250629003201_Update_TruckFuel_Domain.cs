using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TruckFuel_Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "TruckLocationId",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TruckFuelHistory_TruckLocationId",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                column: "TruckLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckFuelHistory_TruckLocationHistory_TruckLocationId",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                column: "TruckLocationId",
                principalSchema: "TuckTracking",
                principalTable: "TruckLocationHistory",
                principalColumn: "TruckLocationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckFuelHistory_TruckLocationHistory_TruckLocationId",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.DropIndex(
                name: "IX_TruckFuelHistory_TruckLocationId",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.DropColumn(
                name: "TruckLocationId",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                type: "numeric(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                type: "numeric(9,6)",
                nullable: true);
        }
    }
}
