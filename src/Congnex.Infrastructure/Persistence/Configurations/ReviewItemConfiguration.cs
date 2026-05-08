using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class ReviewItemConfiguration : IEntityTypeConfiguration<ReviewItem>
{
    public void Configure(EntityTypeBuilder<ReviewItem> b)
    {
        b.ToTable("review_items");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id");
        b.Property(r => r.UserId).HasColumnName("user_id");
        b.Property(r => r.QuestionId).HasColumnName("question_id");
        b.Property(r => r.AiQuestionId).HasColumnName("ai_question_id");
        b.Property(r => r.Source).HasColumnName("source").HasMaxLength(20);
        b.Property(r => r.Stability).HasColumnName("stability");
        b.Property(r => r.Difficulty).HasColumnName("difficulty").HasDefaultValue(5f);
        b.Property(r => r.DueDate).HasColumnName("due_date");
        b.Property(r => r.LastReviewAt).HasColumnName("last_review_at");
        b.Property(r => r.Reps).HasColumnName("reps").HasDefaultValue(0);
        b.Property(r => r.Lapses).HasColumnName("lapses").HasDefaultValue(0);
        b.Property(r => r.State).HasColumnName("state")
            .HasConversion<string>().HasDefaultValue(FsrsCardState.New);
        b.Property(r => r.CreatedAt).HasColumnName("created_at");
        b.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(r => new { r.UserId, r.DueDate });

        b.HasOne(r => r.User).WithMany(u => u.ReviewItems)
            .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(r => r.Question).WithMany(q => q.ReviewItems)
            .HasForeignKey(r => r.QuestionId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(r => r.AiQuestion).WithMany(q => q.ReviewItems)
            .HasForeignKey(r => r.AiQuestionId).OnDelete(DeleteBehavior.SetNull);
    }
}
