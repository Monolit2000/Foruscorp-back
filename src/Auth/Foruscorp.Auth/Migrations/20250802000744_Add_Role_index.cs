using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Auth.Migrations
{
    /// <inheritdoc />
    public partial class Add_Role_index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                schema: "User",
                table: "UserRoles");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_Role",
                schema: "User",
                table: "UserRoles",
                columns: new[] { "UserId", "Role" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_Role",
                schema: "User",
                table: "UserRoles");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                schema: "User",
                table: "UserRoles",
                column: "UserId");
        }
    }
}
