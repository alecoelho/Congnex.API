using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> b)
    {
        b.ToTable("questions");
        b.HasKey(q => q.Id);

        b.Property(q => q.Id).HasColumnName("id");
        b.Property(q => q.LessonId).HasColumnName("lesson_id").IsRequired();
        b.Property(q => q.VideoId).HasColumnName("video_id");
        b.Property(q => q.LearningItemId).HasColumnName("learning_item_id");
        b.Property(q => q.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
        b.Property(q => q.Label).HasColumnName("label").HasMaxLength(500);
        b.Property(q => q.Prompt).HasColumnName("prompt").HasMaxLength(2000);
        b.Property(q => q.Instruction).HasColumnName("instruction").HasMaxLength(1000);
        b.Property(q => q.QuestionText).HasColumnName("question_text").HasMaxLength(2000).IsRequired();
        b.Property(q => q.CorrectAnswer).HasColumnName("correct_answer").HasMaxLength(1000);
        b.Property(q => q.AudioText).HasColumnName("audio_text").HasMaxLength(2000);
        b.Property(q => q.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        b.Property(q => q.OrderIndex).HasColumnName("order_index").IsRequired();
        b.Property(q => q.Difficulty).HasColumnName("difficulty").HasMaxLength(20).HasDefaultValue("easy");
        b.Property(q => q.CreatedAt).HasColumnName("created_at");
        b.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(q => q.LessonId);
        b.HasIndex(q => q.VideoId);
        b.HasIndex(q => q.LearningItemId);

        b.HasOne(q => q.Lesson)
         .WithMany(l => l.Questions)
         .HasForeignKey(q => q.LessonId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(q => q.Video)
         .WithMany(v => v.Questions)
         .HasForeignKey(q => q.VideoId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(q => q.LearningItem)
         .WithMany(i => i.Questions)
         .HasForeignKey(q => q.LearningItemId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
