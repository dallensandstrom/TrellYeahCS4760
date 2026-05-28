using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedGrantFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IRBApprovalFilePath",
                table: "Grants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportingDocument1Path",
                table: "Grants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportingDocument2Path",
                table: "Grants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportingDocument3Path",
                table: "Grants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IRBApprovalFilePath",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "SupportingDocument1Path",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "SupportingDocument2Path",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "SupportingDocument3Path",
                table: "Grants");
        }
    }
}
