using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plans_ShiftTemplates_ShiftId",
                table: "Plans");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "EndUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Locality",
                table: "EndUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Localities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Localities_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Pune" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Mumbai" }
                });

            migrationBuilder.InsertData(
                table: "Localities",
                columns: new[] { "Id", "CityId", "Name" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), "Kothrud" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), "Viman Nagar" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), "Hinjewadi" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), "Wakad" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new Guid("11111111-1111-1111-1111-111111111111"), "Baner" },
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("22222222-2222-2222-2222-222222222222"), "Andheri" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("22222222-2222-2222-2222-222222222222"), "Bandra" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("22222222-2222-2222-2222-222222222222"), "Borivali" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("22222222-2222-2222-2222-222222222222"), "Dadar" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new Guid("22222222-2222-2222-2222-222222222222"), "Juhu" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Localities_CityId",
                table: "Localities",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_ShiftTemplates_ShiftId",
                table: "Plans",
                column: "ShiftId",
                principalTable: "ShiftTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plans_ShiftTemplates_ShiftId",
                table: "Plans");

            migrationBuilder.DropTable(
                name: "Localities");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropColumn(
                name: "City",
                table: "EndUsers");

            migrationBuilder.DropColumn(
                name: "Locality",
                table: "EndUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_ShiftTemplates_ShiftId",
                table: "Plans",
                column: "ShiftId",
                principalTable: "ShiftTemplates",
                principalColumn: "Id");
        }
    }
}
