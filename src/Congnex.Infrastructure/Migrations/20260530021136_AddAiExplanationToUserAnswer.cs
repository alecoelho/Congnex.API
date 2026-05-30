using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiExplanationToUserAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiExplanation",
                table: "user_question_answers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiExplanation",
                table: "user_question_answers");
        }
    }
}
