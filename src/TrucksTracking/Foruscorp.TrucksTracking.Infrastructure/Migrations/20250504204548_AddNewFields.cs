using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderTruckId",
                schema: "TuckTracking",
                table: "TruckTrackers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderTruckId",
                schema: "TuckTracking",
                table: "TruckTrackers");
        }
    }
}
