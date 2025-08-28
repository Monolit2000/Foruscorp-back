using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Forward_Distacett : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

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
