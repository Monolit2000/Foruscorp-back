using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class modelchanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyManager_Companys_CompanyId",
                schema: "Tuck",
                table: "CompanyManager");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyManager_Users_UserId",
                schema: "Tuck",
                table: "CompanyManager");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyManager",
                schema: "Tuck",
                table: "CompanyManager");

            migrationBuilder.RenameTable(
                name: "CompanyManager",
                schema: "Tuck",
                newName: "CompanyManagers",
                newSchema: "Tuck");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyManager_UserId_CompanyId",
                schema: "Tuck",
                table: "CompanyManagers",
                newName: "IX_CompanyManagers_UserId_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyManager_UserId",
                schema: "Tuck",
                table: "CompanyManagers",
                newName: "IX_CompanyManagers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyManager_CompanyId",
                schema: "Tuck",
                table: "CompanyManagers",
                newName: "IX_CompanyManagers_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyManagers",
                schema: "Tuck",
                table: "CompanyManagers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyManagers_Companys_CompanyId",
                schema: "Tuck",
                table: "CompanyManagers",
                column: "CompanyId",
                principalSchema: "Tuck",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyManagers_Users_UserId",
                schema: "Tuck",
                table: "CompanyManagers",
                column: "UserId",
                principalSchema: "Tuck",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyManagers_Companys_CompanyId",
                schema: "Tuck",
                table: "CompanyManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyManagers_Users_UserId",
                schema: "Tuck",
                table: "CompanyManagers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyManagers",
                schema: "Tuck",
                table: "CompanyManagers");

            migrationBuilder.RenameTable(
                name: "CompanyManagers",
                schema: "Tuck",
                newName: "CompanyManager",
                newSchema: "Tuck");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyManagers_UserId_CompanyId",
                schema: "Tuck",
                table: "CompanyManager",
                newName: "IX_CompanyManager_UserId_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyManagers_UserId",
                schema: "Tuck",
                table: "CompanyManager",
                newName: "IX_CompanyManager_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyManagers_CompanyId",
                schema: "Tuck",
                table: "CompanyManager",
                newName: "IX_CompanyManager_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyManager",
                schema: "Tuck",
                table: "CompanyManager",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyManager_Companys_CompanyId",
                schema: "Tuck",
                table: "CompanyManager",
                column: "CompanyId",
                principalSchema: "Tuck",
                principalTable: "Companys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyManager_Users_UserId",
                schema: "Tuck",
                table: "CompanyManager",
                column: "UserId",
                principalSchema: "Tuck",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
