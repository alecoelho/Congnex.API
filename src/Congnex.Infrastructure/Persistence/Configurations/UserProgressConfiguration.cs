using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> b)
    {
        b.ToTable("user_progress");
        b.HasKey(p => p.Id);

        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        b.Property(p => p.LessonId).HasColumnName("lesson_id").IsRequired();
        b.Property(p => p.UnitId).HasColumnName("unit_id");
        b.Property(p => p.BlockId).HasColumnName("block_id");
        b.Property(p => p.Status).HasColumnName("status")
            .HasConversion<string>().HasMaxLength(50).IsRequired();
        b.Property(p => p.Score).HasColumnName("score").HasDefaultValue(0);
        b.Property(p => p.CorrectAnswers).HasColumnName("correct_answers").HasDefaultValue(0);
        b.Property(p => p.TotalQuestions).HasColumnName("total_questions").HasDefaultValue(0);
        b.Property(p => p.XpEarned).HasColumnName("xp_earned").HasDefaultValue(0);
        b.Property(p => p.CompletedAt).HasColumnName("completed_at");
        b.Property(p => p.CreatedAt).HasColumnName("created_at");
        b.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(p => p.User)
         .WithMany(u => u.Progress)
         .HasForeignKey(p => p.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(p => p.Lesson)
         .WithMany(l => l.UserProgress)
         .HasForeignKey(p => p.LessonId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(p => p.Unit)
         .WithMany()
         .HasForeignKey(p => p.UnitId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(p => new { p.UserId, p.LessonId }).IsUnique();
    }
}
