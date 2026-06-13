using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class QuestionBank : Entity
{
    public string CefrLevel { get; set; } = string.Empty;     // A1, A2, B1, B2, C1, C2
    public string QuestionType { get; set; } = string.Empty;  // funcoes_comunicativas, vocabulario, gramatica, habilidades_receptivas, completar_frase
    public string Domain { get; set; } = string.Empty;        // rotina_diaria, trabalho_negocios, etc.
    public string Type { get; set; } = string.Empty;          // multiple_choice, translation, complete_sentence
    public string QuestionText { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "easy";          // easy, medium, hard

    public ICollection<QuestionBankOption> Options { get; set; } = [];
}
