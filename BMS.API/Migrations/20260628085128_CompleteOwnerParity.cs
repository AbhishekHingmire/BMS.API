using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS.API.Migrations
{
    /// <inheritdoc />
    public partial class CompleteOwnerParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "OwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "OwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "OwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                table: "OwnerUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IfscCode",
                table: "OwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SmsNotificationsEnabled",
                table: "OwnerUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpiId",
                table: "OwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotosJson",
                table: "Libraries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "OwnerBroadcastHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibraryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedRecipients = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerBroadcastHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerBroadcastHistories_OwnerUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "OwnerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnerNotificationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerNotificationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerNotificationRules_OwnerUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "OwnerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerBroadcastHistories_OwnerId",
                table: "OwnerBroadcastHistories",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerNotificationRules_OwnerId",
                table: "OwnerNotificationRules",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnerBroadcastHistories");

            migrationBuilder.DropTable(
                name: "OwnerNotificationRules");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "IfscCode",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "SmsNotificationsEnabled",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "UpiId",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "PhotosJson",
                table: "Libraries");
        }
    }
}
