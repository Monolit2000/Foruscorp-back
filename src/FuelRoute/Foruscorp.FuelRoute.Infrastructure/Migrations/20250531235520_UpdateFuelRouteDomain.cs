using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFuelRouteDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RouteSections",
                schema: "FuelRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncodeRoute = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteSections_FuelRoutes_RouteId",
                        column: x => x.RouteId,
                        principalSchema: "FuelRoutes",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RouteSections_RouteId",
                schema: "FuelRoutes",
                table: "RouteSections",
                column: "RouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteSections",
                schema: "FuelRoutes");
        }
    }
}
