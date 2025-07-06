using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserId_to_Driver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "Tuck",
                table: "Drivers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                schema: "Tuck",
                table: "Drivers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drivers_UserId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Tuck",
                table: "Drivers");
        }
    }
}
