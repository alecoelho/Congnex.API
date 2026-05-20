using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class XylaConversationConfiguration : IEntityTypeConfiguration<XylaConversation>
{
    public void Configure(EntityTypeBuilder<XylaConversation> b)
    {
        b.ToTable("xyla_conversations");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("id");
        b.Property(c => c.UserId).HasColumnName("user_id");
        b.Property(c => c.SessionId).HasColumnName("session_id");
        b.Property(c => c.DetectedLevel).HasColumnName("detected_level").HasMaxLength(10);
        b.Property(c => c.DetectedGoal).HasColumnName("detected_goal").HasMaxLength(200);
        b.Property(c => c.DetectedInterest).HasColumnName("detected_interest").HasMaxLength(200);
        b.Property(c => c.IsCompleted).HasColumnName("is_completed");
        b.Property(c => c.CompletedAt).HasColumnName("completed_at");
        b.Property(c => c.CreatedAt).HasColumnName("created_at");
        b.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(c => c.UserId);
        b.HasIndex(c => c.SessionId).IsUnique();

        b.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
    }
}
