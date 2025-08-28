using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Locatio_Point_to_RouteSectio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.AddColumn<Guid>(
                name: "FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationPoints_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "FuelRouteSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "RouteId",
                principalSchema: "FuelRoutes",
                principalTable: "FuelRoutes",
                principalColumn: "FuelRouteId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPoints_RouteSections_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "FuelRouteSectionId",
                principalSchema: "FuelRoutes",
                principalTable: "RouteSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationPoints_RouteSections_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.DropIndex(
                name: "IX_LocationPoints_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.DropColumn(
                name: "FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "LocationPoints");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationPoints_FuelRoutes_RouteId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "RouteId",
                principalSchema: "FuelRoutes",
                principalTable: "FuelRoutes",
                principalColumn: "FuelRouteId");
        }
    }
}
