using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Worker.Migrations
{
    /// <inheritdoc />
    public partial class init_worker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TruckTrackerWorker");

            migrationBuilder.CreateTable(
                name: "TruckTrackers",
                schema: "TruckTrackerWorker",
                columns: table => new
                {
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderTruckId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckTrackers", x => x.TruckId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TruckTrackers",
                schema: "TruckTrackerWorker");
        }
    }
}
