using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanEntitlements");

            migrationBuilder.AddColumn<bool>(
                name: "AllowAdvancedQuests",
                table: "Plans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Plans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DailyQuestLimit",
                table: "Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Plans",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Plans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "XpMultiplier",
                table: "Plans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "AllowAdvancedQuests", "CreatedAtUtc", "DailyQuestLimit", "Description", "UpdatedAtUtc", "XpMultiplier" },
                values: new object[] { false, new DateTime(2026, 5, 28, 14, 36, 54, 59, DateTimeKind.Utc).AddTicks(3336), 10, null, new DateTime(2026, 5, 28, 14, 36, 54, 59, DateTimeKind.Utc).AddTicks(3338), 1.0m });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "AllowAdvancedQuests", "CreatedAtUtc", "DailyQuestLimit", "Description", "UpdatedAtUtc", "XpMultiplier" },
                values: new object[] { false, new DateTime(2026, 5, 28, 14, 36, 54, 59, DateTimeKind.Utc).AddTicks(3341), 10, null, new DateTime(2026, 5, 28, 14, 36, 54, 59, DateTimeKind.Utc).AddTicks(3342), 1.0m });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "AllowAdvancedQuests", "CreatedAtUtc", "DailyQuestLimit", "Description", "UpdatedAtUtc", "XpMultiplier" },
                values: new object[] { false, new DateTime(2026, 5, 28, 14, 36, 54, 59, DateTimeKind.Utc).AddTicks(3343), 10, null, new DateTime(2026, 5, 28, 14, 36, 54, 59, DateTimeKind.Utc).AddTicks(3343), 1.0m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAdvancedQuests",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "DailyQuestLimit",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "XpMultiplier",
                table: "Plans");

            migrationBuilder.CreateTable(
                name: "PlanEntitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    EntitlementKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EntitlementValue = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanEntitlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanEntitlements_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanEntitlements_PlanId_EntitlementKey",
                table: "PlanEntitlements",
                columns: new[] { "PlanId", "EntitlementKey" },
                unique: true);
        }
    }
}
