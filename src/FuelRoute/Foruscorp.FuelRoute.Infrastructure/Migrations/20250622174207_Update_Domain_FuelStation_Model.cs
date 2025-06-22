using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Domain_FuelStation_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FuelPrice",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                newName: "PriceAfterDiscount");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "Discount",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.DropColumn(
                name: "Price",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.RenameColumn(
                name: "PriceAfterDiscount",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                newName: "FuelPrice");
        }
    }
}
