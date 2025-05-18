using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_FuelProviderId_to_CompanyPriceMultiplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FuelProviderId",
                schema: "FuelStation",
                table: "CompanyFuelPriceMultipliers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFuelPriceMultipliers_FuelProviderId",
                schema: "FuelStation",
                table: "CompanyFuelPriceMultipliers",
                column: "FuelProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyFuelPriceMultipliers_FuelProviderId",
                schema: "FuelStation",
                table: "CompanyFuelPriceMultipliers");

            migrationBuilder.DropColumn(
                name: "FuelProviderId",
                schema: "FuelStation",
                table: "CompanyFuelPriceMultipliers");
        }
    }
}
