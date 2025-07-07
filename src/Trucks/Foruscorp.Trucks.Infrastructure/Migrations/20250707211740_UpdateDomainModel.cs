using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RouteId",
                schema: "Tuck",
                table: "RouteOffers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TruckId",
                schema: "Tuck",
                table: "RouteOffers",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteId",
                schema: "Tuck",
                table: "RouteOffers");

            migrationBuilder.DropColumn(
                name: "TruckId",
                schema: "Tuck",
                table: "RouteOffers");
        }
    }
}
