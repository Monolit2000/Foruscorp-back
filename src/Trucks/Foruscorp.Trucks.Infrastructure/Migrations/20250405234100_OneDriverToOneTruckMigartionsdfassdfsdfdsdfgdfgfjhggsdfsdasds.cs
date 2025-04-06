using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OneDriverToOneTruckMigartionsdfassdfsdfdsdfgdfgfjhggsdfsdasds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId");
        }
    }
}
