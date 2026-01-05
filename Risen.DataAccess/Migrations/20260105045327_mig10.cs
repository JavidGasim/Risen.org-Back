using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_UserId_CreatedAtUtc",
                table: "XpTransactions");

            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_UserId_SourceKey",
                table: "XpTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "SourceKey",
                table: "XpTransactions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "DifficultyMultiplier",
                table: "XpTransactions",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_XpTransactions_UserId_SourceType_SourceKey",
                table: "XpTransactions",
                columns: new[] { "UserId", "SourceType", "SourceKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XpTransactions_UserId_SourceType_SourceKey",
                table: "XpTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "SourceKey",
                table: "XpTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<decimal>(
                name: "DifficultyMultiplier",
                table: "XpTransactions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,4)",
                oldPrecision: 9,
                oldScale: 4);

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
    }
}
