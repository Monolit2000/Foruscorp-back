using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Rout_to_TruckTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "Routes",
                schema: "TuckTracking",
                columns: table => new
                {
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckTrackerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Routes_TruckTrackers_TruckTrackerId",
                        column: x => x.TruckTrackerId,
                        principalSchema: "TuckTracking",
                        principalTable: "TruckTrackers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Routes_TruckId",
                schema: "TuckTracking",
                table: "Routes",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_TruckTrackerId",
                schema: "TuckTracking",
                table: "Routes",
                column: "TruckTrackerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckTrackers_Routes_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                column: "CurrentRouteId",
                principalSchema: "TuckTracking",
                principalTable: "Routes",
                principalColumn: "RouteId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckTrackers_Routes_CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.DropTable(
                name: "Routes",
                schema: "TuckTracking");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentRouteId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
