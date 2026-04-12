using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RisenScore",
                table: "UserStats",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "LeagueTiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Weight",
                value: 0);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "Weight",
                value: 10);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Weight",
                value: 20);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "Weight",
                value: 35);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Weight",
                value: 50);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Weight",
                value: 70);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Weight",
                value: 90);

            migrationBuilder.UpdateData(
                table: "LeagueTiers",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "Weight",
                value: 120);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RisenScore",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "LeagueTiers");
        }
    }
}
