using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerMarketingFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarketingEmailsEnabled",
                table: "OwnerUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MarketingSmsEnabled",
                table: "OwnerUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MarketingWhatsAppEnabled",
                table: "OwnerUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarketingEmailsEnabled",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "MarketingSmsEnabled",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "MarketingWhatsAppEnabled",
                table: "OwnerUsers");
        }
    }
}
