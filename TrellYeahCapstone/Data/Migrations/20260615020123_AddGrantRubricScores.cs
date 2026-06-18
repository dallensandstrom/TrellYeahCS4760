using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrellYeahCapstone.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrantRubricScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrantRubricScores",
                columns: table => new
                {
                    GrantRubricScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrantId = table.Column<int>(type: "int", nullable: false),
                    RubricCriterionId = table.Column<int>(type: "int", nullable: false),
                    ReviewerUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantRubricScores", x => x.GrantRubricScoreId);
                    table.ForeignKey(
                        name: "FK_GrantRubricScores_AspNetUsers_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrantRubricScores_Grants_GrantId",
                        column: x => x.GrantId,
                        principalTable: "Grants",
                        principalColumn: "GrantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrantRubricScores_RubricCriteria_RubricCriterionId",
                        column: x => x.RubricCriterionId,
                        principalTable: "RubricCriteria",
                        principalColumn: "RubricCriterionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrantRubricScores_GrantId_RubricCriterionId_ReviewerUserId",
                table: "GrantRubricScores",
                columns: new[] { "GrantId", "RubricCriterionId", "ReviewerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrantRubricScores_ReviewerUserId",
                table: "GrantRubricScores",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GrantRubricScores_RubricCriterionId",
                table: "GrantRubricScores",
                column: "RubricCriterionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrantRubricScores");
        }
    }
}
