using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Domain_FuelStation_Modelasdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncodeRoute",
                schema: "FuelRoutes",
                table: "FuelRoutes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncodeRoute",
                schema: "FuelRoutes",
                table: "FuelRoutes",
                type: "text",
                nullable: true);
        }
    }
}
