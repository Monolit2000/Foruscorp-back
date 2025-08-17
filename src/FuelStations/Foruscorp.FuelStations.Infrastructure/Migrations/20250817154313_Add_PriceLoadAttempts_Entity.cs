using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelStations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_PriceLoadAttempts_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceLoadAttempts",
                schema: "FuelStation",
                columns: table => new
                {
                    PriceLoadAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TotalFiles = table.Column<int>(type: "integer", nullable: false),
                    SuccessfullyProcessedFiles = table.Column<int>(type: "integer", nullable: false),
                    FailedFiles = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLoadAttempts", x => x.PriceLoadAttemptId);
                });

            migrationBuilder.CreateTable(
                name: "FileProcessingResults",
                schema: "FuelStation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PriceLoadAttemptId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileProcessingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileProcessingResults_PriceLoadAttempts_PriceLoadAttemptId",
                        column: x => x.PriceLoadAttemptId,
                        principalSchema: "FuelStation",
                        principalTable: "PriceLoadAttempts",
                        principalColumn: "PriceLoadAttemptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileProcessingResults_FileName",
                schema: "FuelStation",
                table: "FileProcessingResults",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_FileProcessingResults_IsSuccess",
                schema: "FuelStation",
                table: "FileProcessingResults",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_FileProcessingResults_PriceLoadAttemptId",
                schema: "FuelStation",
                table: "FileProcessingResults",
                column: "PriceLoadAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_FileProcessingResults_ProcessedAt",
                schema: "FuelStation",
                table: "FileProcessingResults",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLoadAttempts_CompletedAt",
                schema: "FuelStation",
                table: "PriceLoadAttempts",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLoadAttempts_IsSuccessful",
                schema: "FuelStation",
                table: "PriceLoadAttempts",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLoadAttempts_StartedAt",
                schema: "FuelStation",
                table: "PriceLoadAttempts",
                column: "StartedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileProcessingResults",
                schema: "FuelStation");

            migrationBuilder.DropTable(
                name: "PriceLoadAttempts",
                schema: "FuelStation");
        }
    }
}
