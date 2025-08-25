using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Push.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsActive_Device : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "PushNotifications",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "PushNotifications",
                table: "Devices");
        }
    }
}
