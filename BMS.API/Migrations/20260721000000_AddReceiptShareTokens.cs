using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptShareTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceiptShareTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptShareTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptShareTokens_BookingId",
                table: "ReceiptShareTokens",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptShareTokens_Token",
                table: "ReceiptShareTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptShareTokens");
        }
    }
}
