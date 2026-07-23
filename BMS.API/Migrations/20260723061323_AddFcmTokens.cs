using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFcmTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "OwnerUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "EndUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "OwnerUsers");

            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "EndUsers");
        }
    }
}
