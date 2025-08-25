using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_FuelRouteStation_Add_IsAssigned_IsPlaned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAssigned",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlaned",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAssigned",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "IsPlaned",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");
        }
    }
}
