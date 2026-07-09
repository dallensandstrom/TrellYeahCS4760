using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixGrantReportGrantRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrantReports_GrantAllocations_GrantId",
                table: "GrantReports");

            migrationBuilder.AddForeignKey(
                name: "FK_GrantReports_Grants_GrantId",
                table: "GrantReports",
                column: "GrantId",
                principalTable: "Grants",
                principalColumn: "GrantId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrantReports_Grants_GrantId",
                table: "GrantReports");

            migrationBuilder.AddForeignKey(
                name: "FK_GrantReports_GrantAllocations_GrantId",
                table: "GrantReports",
                column: "GrantId",
                principalTable: "GrantAllocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
