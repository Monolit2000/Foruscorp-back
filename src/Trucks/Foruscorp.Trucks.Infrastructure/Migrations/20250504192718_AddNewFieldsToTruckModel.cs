using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToTruckModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Tuck",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Year",
                schema: "Tuck",
                table: "Trucks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Year",
                schema: "Tuck",
                table: "Trucks");
        }
    }
}
