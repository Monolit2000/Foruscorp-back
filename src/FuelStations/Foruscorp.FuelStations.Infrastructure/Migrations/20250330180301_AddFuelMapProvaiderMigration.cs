using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelMapProvaiderMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuelMapProvaiders",
                schema: "FuelStation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RefreshedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelMapProvaiders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelMapProvaiders_Name",
                schema: "FuelStation",
                table: "FuelMapProvaiders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelMapProvaiders_Url",
                schema: "FuelStation",
                table: "FuelMapProvaiders",
                column: "Url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelMapProvaiders",
                schema: "FuelStation");
        }
    }
}
