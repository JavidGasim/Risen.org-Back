using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStreak",
                table: "UserStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStreakDateUtc",
                table: "UserStats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LongestStreak",
                table: "UserStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    BaseXp = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPremiumOnly = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AwardedXp = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestAttempts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestAttempts_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_QuestId",
                table: "QuestAttempts",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_UserId_CompletedAtUtc",
                table: "QuestAttempts",
                columns: new[] { "UserId", "CompletedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_UserId_QuestId_CompletedDateUtc",
                table: "QuestAttempts",
                columns: new[] { "UserId", "QuestId", "CompletedDateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quests_SubjectCode_Difficulty_IsActive",
                table: "Quests",
                columns: new[] { "SubjectCode", "Difficulty", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestAttempts");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropColumn(
                name: "CurrentStreak",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "LastStreakDateUtc",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "LongestStreak",
                table: "UserStats");
        }
    }
}
