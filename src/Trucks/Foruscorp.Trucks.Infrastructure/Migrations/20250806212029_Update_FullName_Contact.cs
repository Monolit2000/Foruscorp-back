using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_FullName_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "Tuck",
                table: "Drivers",
                newName: "Contact_FullName");

            migrationBuilder.AlterColumn<string>(
                name: "Contact_FullName",
                schema: "Tuck",
                table: "Drivers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Contact_FullName",
                schema: "Tuck",
                table: "Drivers",
                newName: "FullName");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "Tuck",
                table: "Drivers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
