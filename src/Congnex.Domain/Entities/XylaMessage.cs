using Congnex.Domain.Common;

namespace Congnex.Domain.Entities;

public class XylaMessage : Entity
{
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;

    public XylaConversation Conversation { get; set; } = null!;
}
