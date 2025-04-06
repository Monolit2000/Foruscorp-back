using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OneDriverToOneTruckMigartionsdfassdfsdfdsdfgdfgfjhggsdfsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Drivers_DriverId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers",
                column: "TruckId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Trucks_TruckId",
                schema: "Tuck",
                table: "Drivers",
                column: "TruckId",
                principalSchema: "Tuck",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Trucks_TruckId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers",
                column: "TruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Drivers_DriverId",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId",
                principalSchema: "Tuck",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
