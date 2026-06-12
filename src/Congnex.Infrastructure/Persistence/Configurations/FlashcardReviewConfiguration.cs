using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class FlashcardReviewConfiguration : IEntityTypeConfiguration<FlashcardReview>
{
    public void Configure(EntityTypeBuilder<FlashcardReview> b)
    {
        b.ToTable("flashcard_reviews");
        b.HasKey(r => r.Id);

        b.Property(r => r.Id).HasColumnName("id");
        b.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        b.Property(r => r.LessonId).HasColumnName("lesson_id").IsRequired();
        b.Property(r => r.Word).HasColumnName("word").HasMaxLength(100).IsRequired();
        b.Property(r => r.Remembered).HasColumnName("remembered").IsRequired();
        b.Property(r => r.ReviewCount).HasColumnName("review_count").HasDefaultValue(1);
        b.Property(r => r.CorrectCount).HasColumnName("correct_count").HasDefaultValue(0);
        b.Property(r => r.LastReviewedAt).HasColumnName("last_reviewed_at").IsRequired();
        b.Property(r => r.NextReviewAt).HasColumnName("next_review_at").IsRequired();
        b.Property(r => r.IntervalDays).HasColumnName("interval_days").HasDefaultValue(1);
        b.Property(r => r.CreatedAt).HasColumnName("created_at");
        b.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(r => r.User)
         .WithMany()
         .HasForeignKey(r => r.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(r => r.Lesson)
         .WithMany()
         .HasForeignKey(r => r.LessonId)
         .OnDelete(DeleteBehavior.Cascade);

        // One review record per user+word
        b.HasIndex(r => new { r.UserId, r.Word }).IsUnique();

        // For querying due reviews
        b.HasIndex(r => new { r.UserId, r.NextReviewAt });
    }
}
