using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaPlanOverrideIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShiftOverrideId",
                table: "Areas");

            migrationBuilder.AddColumn<string>(
                name: "PlanOverrideIdsJson",
                table: "Areas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanOverrideIdsJson",
                table: "Areas");

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftOverrideId",
                table: "Areas",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
