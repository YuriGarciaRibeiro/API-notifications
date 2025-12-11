using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_channels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    to = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    body = table.Column<string>(type: "text", nullable: true),
                    is_body_html = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    content_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    content_body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content_click_action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: true),
                    condition = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    time_to_live = table.Column<int>(type: "integer", nullable: true),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mutable_content = table.Column<bool>(type: "boolean", nullable: true),
                    content_available = table.Column<bool>(type: "boolean", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    message = table.Column<string>(type: "character varying(1600)", maxLength: 1600, nullable: true),
                    sender_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    android_config = table.Column<string>(type: "jsonb", nullable: true),
                    apns_config = table.Column<string>(type: "jsonb", nullable: true),
                    webpush_config = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_channels", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_channels_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_channels_notification_id",
                table: "notification_channels",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_notification_id_type",
                table: "notification_channels",
                columns: new[] { "notification_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_channels_status",
                table: "notification_channels",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_at",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_channels");

            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
