using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemuveUlidPropertyFromTruckEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trucks_Ulid",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Ulid",
                schema: "Tuck",
                table: "Trucks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ulid",
                schema: "Tuck",
                table: "Trucks",
                type: "character varying(26)",
                maxLength: 26,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Ulid",
                schema: "Tuck",
                table: "Trucks",
                column: "Ulid");
        }
    }
}
