using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trucks_LicensePlate",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_Ulid",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                schema: "Tuck",
                table: "Trucks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtTime",
                schema: "Tuck",
                table: "Trucks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "HarshAccelerationSettingType",
                schema: "Tuck",
                table: "Trucks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Make",
                schema: "Tuck",
                table: "Trucks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                schema: "Tuck",
                table: "Trucks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderTruckId",
                schema: "Tuck",
                table: "Trucks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Serial",
                schema: "Tuck",
                table: "Trucks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtTime",
                schema: "Tuck",
                table: "Trucks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Vin",
                schema: "Tuck",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_LicensePlate",
                schema: "Tuck",
                table: "Trucks",
                column: "LicensePlate");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Serial",
                schema: "Tuck",
                table: "Trucks",
                column: "Serial");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Ulid",
                schema: "Tuck",
                table: "Trucks",
                column: "Ulid");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Vin",
                schema: "Tuck",
                table: "Trucks",
                column: "Vin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trucks_LicensePlate",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_Serial",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_Ulid",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_Vin",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "CreatedAtTime",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "HarshAccelerationSettingType",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Make",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Model",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "ProviderTruckId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Serial",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "UpdatedAtTime",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Vin",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                schema: "Tuck",
                table: "Trucks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_LicensePlate",
                schema: "Tuck",
                table: "Trucks",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Ulid",
                schema: "Tuck",
                table: "Trucks",
                column: "Ulid",
                unique: true);
        }
    }
}
