using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.FuelRoutes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Forward_Distace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RouteValidators_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "RouteValidators");

            migrationBuilder.AlterColumn<Guid>(
                name: "FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FuelRouteId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ForwardDistance",
                schema: "FuelRoutes",
                table: "RouteFuelPoints",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_RouteValidators_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                column: "FuelRouteSectionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RouteValidators_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "RouteValidators");

            migrationBuilder.DropColumn(
                name: "ForwardDistance",
                schema: "FuelRoutes",
                table: "RouteFuelPoints");

            migrationBuilder.AlterColumn<Guid>(
                name: "FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "FuelRouteId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_RouteValidators_FuelRouteSectionId",
                schema: "FuelRoutes",
                table: "RouteValidators",
                column: "FuelRouteSectionId");
        }
    }
}
