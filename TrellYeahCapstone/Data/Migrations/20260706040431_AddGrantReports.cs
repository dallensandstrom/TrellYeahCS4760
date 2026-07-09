using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrantReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AwardDate",
                table: "Grants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GrantReports",
                columns: table => new
                {
                    GrantReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrantId = table.Column<int>(type: "int", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectSummary = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    CurrentProgress = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    NextSteps = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Budget = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    ReportFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantReports", x => x.GrantReportId);
                    table.ForeignKey(
                        name: "FK_GrantReports_GrantAllocations_GrantId",
                        column: x => x.GrantId,
                        principalTable: "GrantAllocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrantReports_GrantId",
                table: "GrantReports",
                column: "GrantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrantReports");

            migrationBuilder.DropColumn(
                name: "AwardDate",
                table: "Grants");
        }
    }
}
