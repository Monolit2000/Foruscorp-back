using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_AssignedAt_AcceptedAt_Field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                schema: "FuelRoutes",
                table: "RouteSections",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                schema: "FuelRoutes",
                table: "RouteSections");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                schema: "FuelRoutes",
                table: "RouteSections");
        }
    }
}
