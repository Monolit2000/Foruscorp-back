﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NearFuelStationPlan_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                schema: "TuckTracking",
                table: "NearFuelStationPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                schema: "TuckTracking",
                table: "NearFuelStationPlans");
        }
    }
}
