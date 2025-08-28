using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _Locatio_Point_RouteSectionManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "FuelRouteSectionLocationPoints",
                schema: "FuelRoutes",
                columns: table => new
                {
                    FuelRouteSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationPointId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelRouteSectionLocationPoints", x => new { x.FuelRouteSectionId, x.LocationPointId });
                    table.ForeignKey(
                        name: "FK_FuelRouteSectionLocationPoints_LocationPoints_LocationPoint~",
                        column: x => x.LocationPointId,
                        principalSchema: "FuelRoutes",
                        principalTable: "LocationPoints",
                        principalColumn: "LocationPointId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FuelRouteSectionLocationPoints_RouteSections_FuelRouteSecti~",
                        column: x => x.FuelRouteSectionId,
                        principalSchema: "FuelRoutes",
                        principalTable: "RouteSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelRouteSectionLocationPoints_LocationPointId",
                schema: "FuelRoutes",
                table: "FuelRouteSectionLocationPoints",
                column: "LocationPointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelRouteSectionLocationPoints",
                schema: "FuelRoutes");

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
                name: "FK_LocationPoints_RouteSections_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "LocationPoints",
                column: "FuelRouteSectionId",
                principalSchema: "FuelRoutes",
                principalTable: "RouteSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
