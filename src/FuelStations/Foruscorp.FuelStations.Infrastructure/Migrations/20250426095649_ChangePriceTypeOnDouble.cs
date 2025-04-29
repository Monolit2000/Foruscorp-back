using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePriceTypeOnDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Price",
                schema: "FuelStation",
                table: "FuelPrices",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "DiscountedPrice",
                schema: "FuelStation",
                table: "FuelPrices",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "FuelStation",
                table: "FuelPrices",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountedPrice",
                schema: "FuelStation",
                table: "FuelPrices",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);
        }
    }
}
