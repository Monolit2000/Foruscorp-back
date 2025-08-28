using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_FuelStationChange_RouteValidator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RouteValidators",
                schema: "FuelRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelRouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FuelRouteSectionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteValidators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteValidators_FuelRoutes_FuelRouteId",
                        column: x => x.FuelRouteId,
                        principalSchema: "FuelRoutes",
                        principalTable: "FuelRoutes",
                        principalColumn: "FuelRouteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RouteValidators_RouteSections_FuelRouteSectionId",
                        column: x => x.FuelRouteSectionId,
                        principalSchema: "FuelRoutes",
                        principalTable: "RouteSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FuelStationChanges",
                schema: "FuelRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteValidatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelRouteStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ForwardDistance = table.Column<double>(type: "double precision", nullable: false),
                    Refill = table.Column<double>(type: "double precision", nullable: false),
                    CurrentFuel = table.Column<double>(type: "double precision", nullable: false),
                    IsAlgo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelStationChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelStationChanges_RouteFuelPoints_FuelRouteStationId",
                        column: x => x.FuelRouteStationId,
                        principalSchema: "FuelRoutes",
                        principalTable: "RouteFuelPoints",
                        principalColumn: "FuelStationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FuelStationChanges_RouteValidators_RouteValidatorId",
                        column: x => x.RouteValidatorId,
                        principalSchema: "FuelRoutes",
                        principalTable: "RouteValidators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelStationChanges_FuelRouteStationId",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                column: "FuelRouteStationId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelStationChanges_IsAlgo",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                column: "IsAlgo");

            migrationBuilder.CreateIndex(
                name: "IX_FuelStationChanges_IsManual",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                column: "IsManual");

            migrationBuilder.CreateIndex(
                name: "IX_FuelStationChanges_RouteValidatorId",
                schema: "FuelRoutes",
                table: "FuelStationChanges",
                column: "RouteValidatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteValidators_FuelRouteId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                column: "FuelRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteValidators_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                column: "FuelRouteSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteValidators_IsValid",
                schema: "FuelRoutes",
                table: "RouteValidators",
                column: "IsValid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelStationChanges",
                schema: "FuelRoutes");

            migrationBuilder.DropTable(
                name: "RouteValidators",
                schema: "FuelRoutes");
        }
    }
}
