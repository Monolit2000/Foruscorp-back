using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ExternalToken_Company : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalToken",
                schema: "Tuck",
                table: "Companys",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalToken",
                schema: "Tuck",
                table: "Companys");
        }
    }
}
