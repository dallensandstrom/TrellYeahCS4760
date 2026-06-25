using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Grants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArccChairApprovalNotes",
                table: "Grants",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "ArccChairApprovalNotes",
                table: "Grants");
        }
    }
}
