using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IsCompleat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComplet",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComplet",
                schema: "FuelRoutes",
                table: "FuelRoutes");
        }
    }
}
