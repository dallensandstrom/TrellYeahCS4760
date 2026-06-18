using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrantApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeanApprovalNotes",
                table: "Grants",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeptChairApprovalNotes",
                table: "Grants",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasMatchingFunds",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MatchingFundsAmount",
                table: "Grants",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeanApprovalNotes",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "DeptChairApprovalNotes",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "HasMatchingFunds",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "MatchingFundsAmount",
                table: "Grants");
        }
    }
}
