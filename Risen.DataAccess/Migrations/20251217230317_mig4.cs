using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "LeagueTiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MinXp = table.Column<long>(type: "bigint", nullable: false),
                    MaxXp = table.Column<long>(type: "bigint", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XpTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BaseXp = table.Column<int>(type: "int", nullable: false),
                    DifficultyMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalXp = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XpTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromTierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToTierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalXpAtChange = table.Column<long>(type: "bigint", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeagueHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueHistories_LeagueTiers_FromTierId",
                        column: x => x.FromTierId,
                        principalTable: "LeagueTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLeagueHistories_LeagueTiers_ToTierId",
                        column: x => x.ToTierId,
                        principalTable: "LeagueTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserStats",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalXp = table.Column<long>(type: "bigint", nullable: false),
                    CurrentLeagueTierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStats", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserStats_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStats_LeagueTiers_CurrentLeagueTierId",
                        column: x => x.CurrentLeagueTierId,
                        principalTable: "LeagueTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "LeagueTiers",
                columns: new[] { "Id", "Code", "MaxXp", "MinXp", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), 1, 999L, 0L, "Rookie", 1 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), 2, 2999L, 1000L, "Bronze", 2 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), 3, 5999L, 3000L, "Silver", 3 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"), 4, 9999L, 6000L, "Gold", 4 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"), 5, 14999L, 10000L, "Platinum", 5 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6"), 6, 21999L, 15000L, "Diamond", 6 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7"), 7, 29999L, 22000L, "Master", 7 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa8"), 8, null, 30000L, "Legend", 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueTiers_Code",
                table: "LeagueTiers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueHistories_FromTierId",
                table: "UserLeagueHistories",
                column: "FromTierId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueHistories_ToTierId",
                table: "UserLeagueHistories",
                column: "ToTierId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueHistories_UserId_ChangedAtUtc",
                table: "UserLeagueHistories",
                columns: new[] { "UserId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_CurrentLeagueTierId",
                table: "UserStats",
                column: "CurrentLeagueTierId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_TotalXp",
                table: "UserStats",
                column: "TotalXp");

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_UserId_CreatedAtUtc",
                table: "XpTransactions",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_UserId_SourceKey",
                table: "XpTransactions",
                columns: new[] { "UserId", "SourceKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagueHistories");

            migrationBuilder.DropTable(
                name: "UserStats");

            migrationBuilder.DropTable(
                name: "XpTransactions");

            migrationBuilder.DropTable(
                name: "LeagueTiers");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
