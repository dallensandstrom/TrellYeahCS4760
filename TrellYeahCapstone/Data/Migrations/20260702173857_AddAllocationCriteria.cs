using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllocationCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AllocatedAmount",
                table: "Grants",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AllocationCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinScorePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MaxScorePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AllocationPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationCriteria", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllocationCriteria");

            migrationBuilder.DropColumn(
                name: "AllocatedAmount",
                table: "Grants");
        }
    }
}
