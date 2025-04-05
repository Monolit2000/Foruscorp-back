using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DriverIdInDriverBonusEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DriverId1",
                schema: "Tuck",
                table: "Trucks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId1",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Drivers_DriverId1",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId1",
                principalSchema: "Tuck",
                principalTable: "Drivers",
                principalColumn: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Drivers_DriverId1",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId1",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "DriverId1",
                schema: "Tuck",
                table: "Trucks");
        }
    }
}
