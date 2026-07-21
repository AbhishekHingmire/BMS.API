using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerPlanFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "OwnerUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "free");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanStartedAt",
                table: "OwnerUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "PlanStartedAt",
                table: "OwnerUsers");
        }
    }
}
