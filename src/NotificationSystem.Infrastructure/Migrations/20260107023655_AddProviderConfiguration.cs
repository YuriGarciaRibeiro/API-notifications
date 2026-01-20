using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "data",
                table: "notification_channels",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "provider_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_type = table.Column<int>(type: "integer", nullable: false),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    configuration_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_configurations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_provider_configurations_channel_type",
                table: "provider_configurations",
                column: "channel_type");

            migrationBuilder.CreateIndex(
                name: "ix_provider_configurations_channel_type_is_active",
                table: "provider_configurations",
                columns: new[] { "channel_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_provider_configurations_channel_type_is_primary",
                table: "provider_configurations",
                columns: new[] { "channel_type", "is_primary" },
                unique: true,
                filter: "is_primary = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "provider_configurations");

            migrationBuilder.AlterColumn<string>(
                name: "data",
                table: "notification_channels",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
