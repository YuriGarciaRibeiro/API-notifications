using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignSchedulingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Origin",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "BulkNotificationJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurringCron",
                table: "BulkNotificationJobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "BulkNotificationJobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledFor",
                table: "BulkNotificationJobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "BulkNotificationJobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "BulkNotificationJobs");

            migrationBuilder.DropColumn(
                name: "RecurringCron",
                table: "BulkNotificationJobs");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "BulkNotificationJobs");

            migrationBuilder.DropColumn(
                name: "ScheduledFor",
                table: "BulkNotificationJobs");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "BulkNotificationJobs");
        }
    }
}
