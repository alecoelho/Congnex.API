using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class UserQuestionAnswerConfiguration : IEntityTypeConfiguration<UserQuestionAnswer>
{
    public void Configure(EntityTypeBuilder<UserQuestionAnswer> b)
    {
        b.ToTable("user_question_answers");
        b.HasKey(a => a.Id);

        b.Property(a => a.Id).HasColumnName("id");
        b.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        b.Property(a => a.LessonId).HasColumnName("lesson_id").IsRequired();
        b.Property(a => a.QuestionId).HasColumnName("question_id").IsRequired();
        b.Property(a => a.SelectedOptionId).HasColumnName("selected_option_id");
        b.Property(a => a.TextAnswer).HasColumnName("text_answer").HasMaxLength(2000);
        b.Property(a => a.AudioUrl).HasColumnName("audio_url").HasMaxLength(500);
        b.Property(a => a.IsCorrect).HasColumnName("is_correct").HasDefaultValue(false);
        b.Property(a => a.TimeSpentSeconds).HasColumnName("time_spent_seconds").HasDefaultValue(0);
        b.Property(a => a.AnsweredAt).HasColumnName("answered_at").IsRequired();
        b.Property(a => a.CreatedAt).HasColumnName("created_at");
        b.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(a => a.UserId);
        b.HasIndex(a => a.LessonId);
        b.HasIndex(a => a.QuestionId);

        b.HasOne(a => a.User)
         .WithMany(u => u.QuestionAnswers)
         .HasForeignKey(a => a.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(a => a.Lesson)
         .WithMany(l => l.UserQuestionAnswers)
         .HasForeignKey(a => a.LessonId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(a => a.Question)
         .WithMany(q => q.UserAnswers)
         .HasForeignKey(a => a.QuestionId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(a => a.SelectedOption)
         .WithMany()
         .HasForeignKey(a => a.SelectedOptionId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
