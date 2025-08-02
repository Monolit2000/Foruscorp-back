using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Company_Truck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trucks_CompanyId",
                schema: "Tuck",
                table: "Trucks",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Companys_CompanyId",
                schema: "Tuck",
                table: "Trucks",
                column: "CompanyId",
                principalSchema: "Tuck",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Companys_CompanyId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_CompanyId",
                schema: "Tuck",
                table: "Trucks");
        }
    }
}
