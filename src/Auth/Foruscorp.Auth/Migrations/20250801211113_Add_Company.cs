using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Auth.Migrations
{
    /// <inheritdoc />
    public partial class Add_Company : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companys",
                schema: "User",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companys", x => x.CompanyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                schema: "User",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companys_CompanyId",
                schema: "User",
                table: "Users",
                column: "CompanyId",
                principalSchema: "User",
                principalTable: "Companys",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companys_CompanyId",
                schema: "User",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Companys",
                schema: "User");

            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId",
                schema: "User",
                table: "Users");
        }
    }
}
