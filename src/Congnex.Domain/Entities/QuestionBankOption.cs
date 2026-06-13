using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class QuestionBankOption : Entity
{
    public Guid QuestionBankId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }

    public QuestionBank Question { get; set; } = null!;
}
