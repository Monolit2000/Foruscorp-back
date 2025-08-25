using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_TruckUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Truck");

            migrationBuilder.CreateTable(
                name: "ReportLoadAttempts",
                schema: "Tuck",
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
                name: "Transactions",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Card = table.Column<string>(type: "text", nullable: false),
                    Group = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TruckUsages",
                schema: "Truck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckUsages_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "Tuck",
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TruckUsages_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalSchema: "Tuck",
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportFileProcessingResults",
                schema: "Tuck",
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
                        principalSchema: "Tuck",
                        principalTable: "ReportLoadAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fills",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TranDate = table.Column<string>(type: "text", nullable: false),
                    TranTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Invoice = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Driver = table.Column<string>(type: "text", nullable: false),
                    Odometer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fills_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "Tuck",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<double>(type: "double precision", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    DB = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FillId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Fills_FillId",
                        column: x => x.FillId,
                        principalSchema: "Tuck",
                        principalTable: "Fills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fills_CreatedAt",
                schema: "Tuck",
                table: "Fills",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_Driver",
                schema: "Tuck",
                table: "Fills",
                column: "Driver");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_Invoice",
                schema: "Tuck",
                table: "Fills",
                column: "Invoice");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_TransactionId",
                schema: "Tuck",
                table: "Fills",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_Unit",
                schema: "Tuck",
                table: "Fills",
                column: "Unit");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedAt",
                schema: "Tuck",
                table: "Items",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Items_FillId",
                schema: "Tuck",
                table: "Items",
                column: "FillId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Type",
                schema: "Tuck",
                table: "Items",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_FileName",
                schema: "Tuck",
                table: "ReportFileProcessingResults",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_IsSuccess",
                schema: "Tuck",
                table: "ReportFileProcessingResults",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_ProcessedAt",
                schema: "Tuck",
                table: "ReportFileProcessingResults",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFileProcessingResults_ReportLoadAttemptId",
                schema: "Tuck",
                table: "ReportFileProcessingResults",
                column: "ReportLoadAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLoadAttempts_CompletedAt",
                schema: "Tuck",
                table: "ReportLoadAttempts",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLoadAttempts_IsSuccessful",
                schema: "Tuck",
                table: "ReportLoadAttempts",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLoadAttempts_StartedAt",
                schema: "Tuck",
                table: "ReportLoadAttempts",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Card",
                schema: "Tuck",
                table: "Transactions",
                column: "Card");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                schema: "Tuck",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Group",
                schema: "Tuck",
                table: "Transactions",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_TruckUsages_DriverId",
                schema: "Truck",
                table: "TruckUsages",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckUsages_TruckId",
                schema: "Truck",
                table: "TruckUsages",
                column: "TruckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "ReportFileProcessingResults",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "TruckUsages",
                schema: "Truck");

            migrationBuilder.DropTable(
                name: "Fills",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "ReportLoadAttempts",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "Tuck");
        }
    }
}
