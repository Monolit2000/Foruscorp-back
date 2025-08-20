using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.TrucksTracking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Trunssaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "TuckTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Card = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Summaries = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fills",
                schema: "TuckTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TranDate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TranTime = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Invoice = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Driver = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Odometer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fills_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "TuckTracking",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                schema: "TuckTracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<double>(type: "numeric(18,4)", nullable: false),
                    Quantity = table.Column<double>(type: "numeric(18,4)", nullable: false),
                    Amount = table.Column<double>(type: "numeric(18,4)", nullable: false),
                    DB = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FillId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Fills_FillId",
                        column: x => x.FillId,
                        principalSchema: "TuckTracking",
                        principalTable: "Fills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fills_CreatedAt",
                schema: "TuckTracking",
                table: "Fills",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_Driver",
                schema: "TuckTracking",
                table: "Fills",
                column: "Driver");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_Invoice",
                schema: "TuckTracking",
                table: "Fills",
                column: "Invoice");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_TransactionId",
                schema: "TuckTracking",
                table: "Fills",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Fills_Unit",
                schema: "TuckTracking",
                table: "Fills",
                column: "Unit");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedAt",
                schema: "TuckTracking",
                table: "Items",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Items_FillId",
                schema: "TuckTracking",
                table: "Items",
                column: "FillId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Type",
                schema: "TuckTracking",
                table: "Items",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Card",
                schema: "TuckTracking",
                table: "Transactions",
                column: "Card");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                schema: "TuckTracking",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Group",
                schema: "TuckTracking",
                table: "Transactions",
                column: "Group");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items",
                schema: "TuckTracking");

            migrationBuilder.DropTable(
                name: "Fills",
                schema: "TuckTracking");

            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "TuckTracking");
        }
    }
}
