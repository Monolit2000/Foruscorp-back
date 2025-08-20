using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ReportLoadAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportLoadAttempts",
                schema: "TuckTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_ReportLoadAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportFileProcessingResults",
                schema: "TuckTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportLoadAttemptId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFileProcessingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportFileProcessingResults_ReportLoadAttempts_ReportLoadAt~",
                        column: x => x.ReportLoadAttemptId,
                        principalSchema: "TuckTracking",
                        principalTable: "ReportLoadAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_FileName",
                schema: "TuckTracking",
                table: "ReportFileProcessingResults",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_IsSuccess",
                schema: "TuckTracking",
                table: "ReportFileProcessingResults",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_ProcessedAt",
                schema: "TuckTracking",
                table: "ReportFileProcessingResults",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_ReportLoadAttemptId",
                schema: "TuckTracking",
                table: "ReportFileProcessingResults",
                column: "ReportLoadAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLoadAttempts_CompletedAt",
                schema: "TuckTracking",
                table: "ReportLoadAttempts",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLoadAttempts_IsSuccessful",
                schema: "TuckTracking",
                table: "ReportLoadAttempts",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLoadAttempts_StartedAt",
                schema: "TuckTracking",
                table: "ReportLoadAttempts",
                column: "StartedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportFileProcessingResults",
                schema: "TuckTracking");

            migrationBuilder.DropTable(
                name: "ReportLoadAttempts",
                schema: "TuckTracking");
        }
    }
}
