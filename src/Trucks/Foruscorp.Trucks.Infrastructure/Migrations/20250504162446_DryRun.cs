using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DryRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProviderTruckId",
                schema: "Tuck",
                table: "Trucks",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProviderTruckId",
                schema: "Tuck",
                table: "Trucks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
