using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BulkNotificationItems_bulkNotificationJobs_BulkJobId",
                table: "BulkNotificationItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bulkNotificationJobs",
                table: "bulkNotificationJobs");

            migrationBuilder.RenameTable(
                name: "bulkNotificationJobs",
                newName: "BulkNotificationJobs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BulkNotificationJobs",
                table: "BulkNotificationJobs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BulkNotificationItems_BulkNotificationJobs_BulkJobId",
                table: "BulkNotificationItems",
                column: "BulkJobId",
                principalTable: "BulkNotificationJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BulkNotificationItems_BulkNotificationJobs_BulkJobId",
                table: "BulkNotificationItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BulkNotificationJobs",
                table: "BulkNotificationJobs");

            migrationBuilder.RenameTable(
                name: "BulkNotificationJobs",
                newName: "bulkNotificationJobs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bulkNotificationJobs",
                table: "bulkNotificationJobs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BulkNotificationItems_bulkNotificationJobs_BulkJobId",
                table: "BulkNotificationItems",
                column: "BulkJobId",
                principalTable: "bulkNotificationJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
