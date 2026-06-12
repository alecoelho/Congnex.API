using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class Unit : Entity
{
    public string LanguageCode { get; set; } = "en";
    public int OrderIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Lesson> Lessons { get; set; } = [];
}
