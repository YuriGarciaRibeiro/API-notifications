using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeConfigurationJsonToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "configuration_json",
                table: "provider_configurations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "configuration_json",
                table: "provider_configurations",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
