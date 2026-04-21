using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    public partial class create_xp_archive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "XpTransactionArchives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BaseXp = table.Column<int>(type: "int", nullable: false),
                    DifficultyMultiplier = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    FinalXp = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdminReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ArchivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpTransactionArchives", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactionArchives_UserId",
                table: "XpTransactionArchives",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactionArchives_AdminId",
                table: "XpTransactionArchives",
                column: "AdminId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "XpTransactionArchives");
        }
    }
}
