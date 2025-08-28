using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Locatio_Point_Route : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuelRoutes_LocationPoints_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropForeignKey(
                name: "FK_FuelRoutes_LocationPoints_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropIndex(
                name: "IX_FuelRoutes_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropIndex(
                name: "IX_FuelRoutes_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropColumn(
                name: "DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");

            migrationBuilder.DropColumn(
                name: "OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelRoutes_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelRoutes_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "OriginLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelRoutes_LocationPoints_DestinationLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "DestinationLocationId",
                principalSchema: "FuelRoutes",
                principalTable: "LocationPoints",
                principalColumn: "LocationPointId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelRoutes_LocationPoints_OriginLocationId",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                column: "OriginLocationId",
                principalSchema: "FuelRoutes",
                principalTable: "LocationPoints",
                principalColumn: "LocationPointId");
        }
    }
}
