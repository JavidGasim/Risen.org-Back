using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Risen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class mig20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanEntitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntitlementKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EntitlementValue = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanEntitlements");
        }
    }
}
