using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Congnex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankFulltext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índice FULLTEXT para busca de questões por relevância da transcrição do vídeo.
            // (EF não modela FULLTEXT nativamente — aplicado via SQL.)
            migrationBuilder.Sql(
                "ALTER TABLE question_bank ADD FULLTEXT INDEX ft_qb (question_text, correct_answer);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE question_bank DROP INDEX ft_qb;");
        }
    }
}
