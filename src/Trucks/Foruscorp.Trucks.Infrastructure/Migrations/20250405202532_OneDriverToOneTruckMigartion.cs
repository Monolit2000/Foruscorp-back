using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OneDriverToOneTruckMigartion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Drivers_DriverId1",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId1",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DriverId1",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.AlterColumn<Guid>(
                name: "DriverId",
                schema: "Tuck",
                table: "Trucks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

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
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.AlterColumn<Guid>(
                name: "DriverId",
                schema: "Tuck",
                table: "Trucks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers",
                column: "TruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Drivers_DriverId1",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId1",
                principalSchema: "Tuck",
                principalTable: "Drivers",
                principalColumn: "DriverId");
        }
    }
}
