using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foruscorp.Push.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PushNotifications");

            migrationBuilder.CreateTable(
                name: "Devices",
                schema: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PushToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                schema: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalSchema: "PushNotifications",
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "PushNotifications",
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_DeviceId",
                schema: "PushNotifications",
                table: "NotificationRecipients",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_NotificationId",
                schema: "PushNotifications",
                table: "NotificationRecipients",
                column: "NotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationRecipients",
                schema: "PushNotifications");

            migrationBuilder.DropTable(
                name: "Devices",
                schema: "PushNotifications");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "PushNotifications");
        }
    }
}
