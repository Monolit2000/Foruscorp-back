using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ModelTruckGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModelTruckGroupId",
                schema: "Tuck",
                table: "Trucks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModelTruckGroups",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckGrouName = table.Column<string>(type: "text", nullable: false),
                    Make = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Year = table.Column<string>(type: "text", nullable: false),
                    AverageFuelConsumption = table.Column<double>(type: "numeric(18,2)", nullable: false),
                    AveregeWeight = table.Column<double>(type: "double precision", nullable: false),
                    FuelCapacity = table.Column<double>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelTruckGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_ModelTruckGroupId",
                schema: "Tuck",
                table: "Trucks",
                column: "ModelTruckGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTruckGroups_Make",
                schema: "Tuck",
                table: "ModelTruckGroups",
                column: "Make");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTruckGroups_Model",
                schema: "Tuck",
                table: "ModelTruckGroups",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTruckGroups_TruckGrouName",
                schema: "Tuck",
                table: "ModelTruckGroups",
                column: "TruckGrouName");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTruckGroups_Year",
                schema: "Tuck",
                table: "ModelTruckGroups",
                column: "Year");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_ModelTruckGroups_ModelTruckGroupId",
                schema: "Tuck",
                table: "Trucks",
                column: "ModelTruckGroupId",
                principalSchema: "Tuck",
                principalTable: "ModelTruckGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_ModelTruckGroups_ModelTruckGroupId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropTable(
                name: "ModelTruckGroups",
                schema: "Tuck");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_ModelTruckGroupId",
                schema: "Tuck",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "ModelTruckGroupId",
                schema: "Tuck",
                table: "Trucks");
        }
    }
}
