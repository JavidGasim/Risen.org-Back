using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_UserId_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropColumn(
                name: "EarnedXp",
                table: "QuestAttempts");

            migrationBuilder.DropColumn(
                name: "SelectedOptionIndex",
                table: "QuestAttempts");

            migrationBuilder.RenameColumn(
                name: "AnsweredAtUtc",
                table: "QuestAttempts",
                newName: "CompletedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Quests",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "QuestOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AwardedXp",
                table: "QuestAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedOptionId",
                table: "QuestAttempts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_QuestId",
                table: "QuestAttempts",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_SelectedOptionId",
                table: "QuestAttempts",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_UserId_QuestId_CompletedDateUtc",
                table: "QuestAttempts",
                columns: new[] { "UserId", "QuestId", "CompletedDateUtc" },
                unique: true,
                filter: "[CompletedDateUtc] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestAttempts_AspNetUsers_UserId",
                table: "QuestAttempts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestAttempts_QuestOptions_SelectedOptionId",
                table: "QuestAttempts",
                column: "SelectedOptionId",
                principalTable: "QuestOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_QuestAttempts_AspNetUsers_UserId",
                table: "QuestAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestAttempts_QuestOptions_SelectedOptionId",
                table: "QuestAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestAttempts_Quests_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_QuestId",
                table: "QuestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_SelectedOptionId",
                table: "QuestAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuestAttempts_UserId_QuestId_CompletedDateUtc",
                table: "QuestAttempts");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "QuestOptions");

            migrationBuilder.DropColumn(
                name: "AwardedXp",
                table: "QuestAttempts");

            migrationBuilder.DropColumn(
                name: "SelectedOptionId",
                table: "QuestAttempts");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "QuestAttempts",
                newName: "AnsweredAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Quests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<int>(
                name: "EarnedXp",
                table: "QuestAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SelectedOptionIndex",
                table: "QuestAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuestAttempts_UserId_QuestId",
                table: "QuestAttempts",
                columns: new[] { "UserId", "QuestId" });
        }
    }
}
