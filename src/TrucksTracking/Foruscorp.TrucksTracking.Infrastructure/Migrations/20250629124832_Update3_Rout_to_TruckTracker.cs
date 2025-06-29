using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update3_Rout_to_TruckTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Routes",
                schema: "TuckTracking",
                table: "Routes");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "TuckTracking",
                table: "Routes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Routes",
                schema: "TuckTracking",
                table: "Routes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Routes",
                schema: "TuckTracking",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "TuckTracking",
                table: "Routes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Routes",
                schema: "TuckTracking",
                table: "Routes",
                column: "RouteId");
        }
    }
}
