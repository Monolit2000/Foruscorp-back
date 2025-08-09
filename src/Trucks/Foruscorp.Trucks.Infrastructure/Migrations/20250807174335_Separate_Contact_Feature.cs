using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Separate_Contact_Feature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact_Email",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Contact_FullName",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Contact_Phone",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Contact_TelegramLink",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                schema: "Tuck",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                schema: "Tuck",
                table: "Drivers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contacts",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelegramLink = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ContactId",
                schema: "Tuck",
                table: "Users",
                column: "ContactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_ContactId",
                schema: "Tuck",
                table: "Drivers",
                column: "ContactId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Contacts_ContactId",
                schema: "Tuck",
                table: "Drivers",
                column: "ContactId",
                principalSchema: "Tuck",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Contacts_ContactId",
                schema: "Tuck",
                table: "Users",
                column: "ContactId",
                principalSchema: "Tuck",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Contacts_ContactId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Contacts_ContactId",
                schema: "Tuck",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Contacts",
                schema: "Tuck");

            migrationBuilder.DropIndex(
                name: "IX_Users_ContactId",
                schema: "Tuck",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_ContactId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "ContactId",
                schema: "Tuck",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContactId",
                schema: "Tuck",
                table: "Drivers");

            migrationBuilder.AddColumn<string>(
                name: "Contact_Email",
                schema: "Tuck",
                table: "Drivers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact_FullName",
                schema: "Tuck",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact_Phone",
                schema: "Tuck",
                table: "Drivers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contact_TelegramLink",
                schema: "Tuck",
                table: "Drivers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
