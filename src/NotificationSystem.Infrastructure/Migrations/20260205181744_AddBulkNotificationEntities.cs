using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkNotificationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "bulkNotificationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessages = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bulkNotificationJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BulkNotificationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BulkJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Recipient = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Variables = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkNotificationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkNotificationItems_bulkNotificationJobs_BulkJobId",
                        column: x => x.BulkJobId,
                        principalTable: "bulkNotificationJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BulkNotificationItems_BulkJobId",
                table: "BulkNotificationItems",
                column: "BulkJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkNotificationItems");

            migrationBuilder.DropTable(
                name: "bulkNotificationJobs");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");
        }
    }
}
