using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_CompanyManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyManager",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyManager", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyManager_Companys_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Tuck",
                        principalTable: "Companys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyManager_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Tuck",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyManager_CompanyId",
                schema: "Tuck",
                table: "CompanyManager",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyManager_UserId",
                schema: "Tuck",
                table: "CompanyManager",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyManager_UserId_CompanyId",
                schema: "Tuck",
                table: "CompanyManager",
                columns: new[] { "UserId", "CompanyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyManager",
                schema: "Tuck");
        }
    }
}
