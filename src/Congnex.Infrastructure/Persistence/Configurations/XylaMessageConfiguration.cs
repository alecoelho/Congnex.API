using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class XylaMessageConfiguration : IEntityTypeConfiguration<XylaMessage>
{
    public void Configure(EntityTypeBuilder<XylaMessage> b)
    {
        b.ToTable("xyla_messages");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).HasColumnName("id");
        b.Property(m => m.ConversationId).HasColumnName("conversation_id");
        b.Property(m => m.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
        b.Property(m => m.Content).HasColumnName("content").HasMaxLength(5000).IsRequired();
        b.Property(m => m.CreatedAt).HasColumnName("created_at");
        b.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(m => m.ConversationId);

        b.HasOne(m => m.Conversation).WithMany(c => c.Messages).HasForeignKey(m => m.ConversationId);
    }
}
