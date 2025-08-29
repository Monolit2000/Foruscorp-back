using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_FuelStationChange_PK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FuelStationChanges",
                schema: "FuelRoutes",
                table: "FuelStationChanges");

            migrationBuilder.AddColumn<Guid>(
                name: "FuelStationChangeId",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FuelStationChanges",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                column: "FuelStationChangeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FuelStationChanges",
                schema: "FuelRoutes",
                table: "FuelStationChanges");

            migrationBuilder.DropColumn(
                name: "FuelStationChangeId",
                schema: "FuelRoutes",
                table: "FuelStationChanges");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FuelStationChanges",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                column: "Id");
        }
    }
}
