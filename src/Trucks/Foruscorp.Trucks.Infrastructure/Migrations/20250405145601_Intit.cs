using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Intit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Tuck");

            migrationBuilder.CreateTable(
                name: "Drivers",
                schema: "Tuck",
                columns: table => new
                {
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: false),
                    Bonus = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.DriverId);
                });

            migrationBuilder.CreateTable(
                name: "Trucks",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ulid = table.Column<string>(type: "character varying(26)", maxLength: 26, nullable: false),
                    LicensePlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverBonuses",
                schema: "Tuck",
                columns: table => new
                {
                    BonusId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverBonuses", x => x.BonusId);
                    table.ForeignKey(
                        name: "FK_DriverBonuses_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "Tuck",
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverFuelHistories",
                schema: "Tuck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverFuelHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverFuelHistories_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "Tuck",
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteOffers",
                schema: "Tuck",
                columns: table => new
                {
                    RouteOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteOffers", x => x.RouteOfferId);
                    table.ForeignKey(
                        name: "FK_RouteOffers_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "Tuck",
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverBonuses_DriverId",
                schema: "Tuck",
                table: "DriverBonuses",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverFuelHistories_DriverId",
                schema: "Tuck",
                table: "DriverFuelHistories",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_LicenseNumber",
                schema: "Tuck",
                table: "Drivers",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_TruckId",
                schema: "Tuck",
                table: "Drivers",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteOffers_DriverId",
                schema: "Tuck",
                table: "RouteOffers",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                schema: "Tuck",
                table: "Trucks",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_LicensePlate",
                schema: "Tuck",
                table: "Trucks",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Ulid",
                schema: "Tuck",
                table: "Trucks",
                column: "Ulid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverBonuses",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "DriverFuelHistories",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "RouteOffers",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "Trucks",
                schema: "Tuck");

            migrationBuilder.DropTable(
                name: "Drivers",
                schema: "Tuck");
        }
    }
}
