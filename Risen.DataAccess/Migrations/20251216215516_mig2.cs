using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UniversityId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StateProvince = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PrimaryDomain = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PrimaryWebPage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    NormalizedKey = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UniversityId",
                table: "AspNetUsers",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_NormalizedKey",
                table: "Universities",
                column: "NormalizedKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Universities_UniversityId",
                table: "AspNetUsers",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Universities_UniversityId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UniversityId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "AspNetUsers");
        }
    }
}
