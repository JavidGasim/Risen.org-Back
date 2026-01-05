using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa7"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa8"));

            migrationBuilder.InsertData(
                table: "LeagueTiers",
                columns: new[] { "Id", "Code", "MaxXp", "MinXp", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), 1, 499L, 0L, "Rookie", 1 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), 2, 1499L, 500L, "Bronze", 2 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), 3, 3499L, 1500L, "Silver", 3 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), 4, 6999L, 3500L, "Gold", 4 },
                    { new Guid("88888888-8888-8888-8888-888888888888"), 5, 11999L, 7000L, "Platinum", 5 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), 6, 19999L, 12000L, "Diamond", 6 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 7, 29999L, 20000L, "Master", 7 },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 8, null, 30000L, "Legend", 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueTiers_SortOrder",
                table: "LeagueTiers",
                column: "SortOrder",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeagueTiers_SortOrder",
                table: "LeagueTiers");

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

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
        }
    }
}
