using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_LocationPoint_to_FuelRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.AlterColumn<Guid>(
                name: "RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "RouteId",
                principalSchema: "FuelRoutes",
                principalTable: "FuelRoutes",
                principalColumn: "FuelRouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.AlterColumn<Guid>(
                name: "RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "RouteId",
                principalSchema: "FuelRoutes",
                principalTable: "FuelRoutes",
                principalColumn: "FuelRouteId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
