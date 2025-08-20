using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeclineFuelRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeclinedAt",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeclined",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclinedAt",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropColumn(
                name: "IsDeclined",
                schema: "FuelRoutes",
                table: "FuelRoutes");
        }
    }
}
