using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRubricCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RubricCriteria",
                columns: table => new
                {
                    RubricCriterionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MaximumScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricCriteria", x => x.RubricCriterionId);
                });

            migrationBuilder.CreateTable(
                name: "RubricRatingSuggestions",
                columns: table => new
                {
                    RubricRatingSuggestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RubricCriterionId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricRatingSuggestions", x => x.RubricRatingSuggestionId);
                    table.ForeignKey(
                        name: "FK_RubricRatingSuggestions_RubricCriteria_RubricCriterionId",
                        column: x => x.RubricCriterionId,
                        principalTable: "RubricCriteria",
                        principalColumn: "RubricCriterionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RubricRatingSuggestions_RubricCriterionId",
                table: "RubricRatingSuggestions",
                column: "RubricCriterionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RubricRatingSuggestions");

            migrationBuilder.DropTable(
                name: "RubricCriteria");
        }
    }
}
