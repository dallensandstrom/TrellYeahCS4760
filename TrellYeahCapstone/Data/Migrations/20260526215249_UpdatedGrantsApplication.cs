using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedGrantsApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BenefitsMultipleDepartments",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                table: "Grants",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDepartmentsBenefited",
                table: "Grants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrincipalInvestigatorUserId",
                table: "Grants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectDirectorUserId",
                table: "Grants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UsesHumanSubjects",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WeberStateStudentsBenefited",
                table: "Grants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenefitsMultipleDepartments",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "Justification",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "NumberOfDepartmentsBenefited",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "PrincipalInvestigatorUserId",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "ProjectDirectorUserId",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "UsesHumanSubjects",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "WeberStateStudentsBenefited",
                table: "Grants");
        }
    }
}
