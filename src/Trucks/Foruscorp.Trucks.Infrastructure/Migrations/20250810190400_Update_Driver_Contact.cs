using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Driver_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Contacts_ContactId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_ContactId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_UserId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "ContactId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                schema: "Tuck",
                table: "Drivers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Users_UserId",
                schema: "Tuck",
                table: "Drivers",
                column: "UserId",
                principalSchema: "Tuck",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Users_UserId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_UserId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                schema: "Tuck",
                table: "Drivers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_ContactId",
                schema: "Tuck",
                table: "Drivers",
                column: "ContactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                schema: "Tuck",
                table: "Drivers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Contacts_ContactId",
                schema: "Tuck",
                table: "Drivers",
                column: "ContactId",
                principalSchema: "Tuck",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
