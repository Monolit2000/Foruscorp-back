using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TruckTrackers_Domain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckFuelHistory_TruckTrackers_TruckId",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropTable(
                name: "CurrentTruckLocations",
                schema: "TuckTracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TruckLocationHistory",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TruckFuelHistory",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "TuckTracking",
                table: "TruckTrackers",
                newName: "TruckTrackerStatus");

            migrationBuilder.AddColumn<int>(
                name: "TruckEngineStates",
                schema: "TuckTracking",
                table: "TruckTrackers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TruckStatus",
                schema: "TuckTracking",
                table: "TruckTrackers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FormattedLocation",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TruckLocationHistory",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "TruckLocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TruckFuelHistory",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                column: "TruckFuelId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLocationHistory_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "TruckTrackerId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLocationHistory_TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "TruckTrackerId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TruckFuelHistory_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                column: "TruckTrackerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TruckFuelHistory_TruckTrackers_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                column: "TruckTrackerId",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "TruckTrackerId",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "TruckTrackerId1",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TruckFuelHistory_TruckTrackers_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TruckLocationHistory",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropIndex(
                name: "IX_TruckLocationHistory_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropIndex(
                name: "IX_TruckLocationHistory_TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TruckFuelHistory",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.DropIndex(
                name: "IX_TruckFuelHistory_TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.DropColumn(
                name: "TruckEngineStates",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.DropColumn(
                name: "TruckStatus",
                schema: "TuckTracking",
                table: "TruckTrackers");

            migrationBuilder.DropColumn(
                name: "FormattedLocation",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropColumn(
                name: "TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropColumn(
                name: "TruckTrackerId1",
                schema: "TuckTracking",
                table: "TruckLocationHistory");

            migrationBuilder.DropColumn(
                name: "TruckTrackerId",
                schema: "TuckTracking",
                table: "TruckFuelHistory");

            migrationBuilder.RenameColumn(
                name: "TruckTrackerStatus",
                schema: "TuckTracking",
                table: "TruckTrackers",
                newName: "Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TruckLocationHistory",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                columns: new[] { "TruckId", "TruckLocationId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TruckFuelHistory",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                columns: new[] { "TruckId", "TruckFuelId" });

            migrationBuilder.CreateTable(
                name: "CurrentTruckLocations",
                schema: "TuckTracking",
                columns: table => new
                {
                    TruckTrackerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentTruckLocations", x => x.TruckTrackerId);
                    table.ForeignKey(
                        name: "FK_CurrentTruckLocations_TruckTrackers_TruckTrackerId",
                        column: x => x.TruckTrackerId,
                        principalSchema: "TuckTracking",
                        principalTable: "TruckTrackers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TruckFuelHistory_TruckTrackers_TruckId",
                schema: "TuckTracking",
                table: "TruckFuelHistory",
                column: "TruckId",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TruckLocationHistory_TruckTrackers_TruckId",
                schema: "TuckTracking",
                table: "TruckLocationHistory",
                column: "TruckId",
                principalSchema: "TuckTracking",
                principalTable: "TruckTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
