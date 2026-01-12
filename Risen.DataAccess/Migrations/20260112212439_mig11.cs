using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestAttempts_Quests_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_UserId_SourceType_SourceKey",
                table: "XpTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Quests_SubjectCode_Difficulty_IsActive",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_UserId_CompletedAtUtc",
                table: "QuestAttempts");

            migrationBuilder.AlterColumn<decimal>(
                name: "DifficultyMultiplier",
                table: "XpTransactions",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,4)",
                oldPrecision: 9,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Quests",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "SubjectCode",
                table: "Quests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Quests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Quests",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedDateUtc",
                table: "QuestAttempts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "MaxXp",
                value: 999L);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 2499L, 1000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 4999L, 2500L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 9999L, 5000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 19999L, 10000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 39999L, 20000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 79999L, 40000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "MinXp",
                value: 80000L);

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
                name: "IX_XpTransactions_UserId_SourceKey",
                table: "XpTransactions",
                columns: new[] { "UserId", "SourceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quests_IsActive",
                table: "Quests",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_SubjectCode",
                table: "Quests",
                column: "SubjectCode");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestAttempts_Quests_QuestId",
                table: "QuestAttempts",
                column: "QuestId",
                principalTable: "Quests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestAttempts_Quests_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_UserId_SourceKey",
                table: "XpTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Quests_IsActive",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_Quests_SubjectCode",
                table: "Quests");

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

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Quests");

            migrationBuilder.AlterColumn<decimal>(
                name: "DifficultyMultiplier",
                table: "XpTransactions",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldPrecision: 6,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Quests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectCode",
                table: "Quests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedDateUtc",
                table: "QuestAttempts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "MaxXp",
                value: 499L);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 1499L, 500L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 3499L, 1500L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 6999L, 3500L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 11999L, 7000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 19999L, 12000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "MaxXp", "MinXp" },
                values: new object[] { 29999L, 20000L });

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "MinXp",
                value: 30000L);

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_UserId_SourceType_SourceKey",
                table: "XpTransactions",
                columns: new[] { "UserId", "SourceType", "SourceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quests_SubjectCode_Difficulty_IsActive",
                table: "Quests",
                columns: new[] { "SubjectCode", "Difficulty", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_UserId_CompletedAtUtc",
                table: "QuestAttempts",
                columns: new[] { "UserId", "CompletedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_QuestAttempts_Quests_QuestId",
                table: "QuestAttempts",
                column: "QuestId",
                principalTable: "Quests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
