using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestAttempts_AspNetUsers_UserId",
                table: "QuestAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestAttempts_Quests_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Quests_IsActive",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_Quests_SubjectCode",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_UserId_QuestId_CompletedDateUtc",
                table: "QuestAttempts");

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"));

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "QuestAttempts",
                newName: "AnsweredAtUtc");

            migrationBuilder.RenameColumn(
                name: "AwardedXp",
                table: "QuestAttempts",
                newName: "SelectedOptionIndex");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Quests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectCode",
                table: "Quests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Quests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(800)",
                oldMaxLength: 800);

            migrationBuilder.AddColumn<int>(
                name: "CorrectOptionIndex",
                table: "Quests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuestionText",
                table: "Quests",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedDateUtc",
                table: "QuestAttempts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "EarnedXp",
                table: "QuestAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "QuestAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "QuestOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestOptions_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_UserId_QuestId",
                table: "QuestAttempts",
                columns: new[] { "UserId", "QuestId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestOptions_QuestId_Index",
                table: "QuestOptions",
                columns: new[] { "QuestId", "Index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_UserId_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropColumn(
                name: "CorrectOptionIndex",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "QuestionText",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "EarnedXp",
                table: "QuestAttempts");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "QuestAttempts");

            migrationBuilder.RenameColumn(
                name: "SelectedOptionIndex",
                table: "QuestAttempts",
                newName: "AwardedXp");

            migrationBuilder.RenameColumn(
                name: "AnsweredAtUtc",
                table: "QuestAttempts",
                newName: "CompletedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Quests",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "SubjectCode",
                table: "Quests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Quests",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedDateUtc",
                table: "QuestAttempts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Quests",
                columns: new[] { "Id", "BaseXp", "CreatedAtUtc", "Description", "Difficulty", "IsActive", "IsPremiumOnly", "SubjectCode", "Title" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Solve 5 basic algebra simplification problems.", 2, true, false, 1, "Basic Algebra Warm-up" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Answer 10 questions on SI units and dimensional analysis.", 2, true, false, 2, "Physics: Units & Dimensions" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Complete 3 array manipulation tasks.", 2, true, false, 3, "Programming: Arrays Basics" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"), 50, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Solve 1 advanced statics problem (forces & moments).", 3, true, true, 5, "Advanced Mechanics Challenge" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quests_IsActive",
                table: "Quests",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_SubjectCode",
                table: "Quests",
                column: "SubjectCode");

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_QuestId",
                table: "QuestAttempts",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_UserId_QuestId_CompletedDateUtc",
                table: "QuestAttempts",
                columns: new[] { "UserId", "QuestId", "CompletedDateUtc" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestAttempts_AspNetUsers_UserId",
                table: "QuestAttempts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestAttempts_Quests_QuestId",
                table: "QuestAttempts",
                column: "QuestId",
                principalTable: "Quests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
