using System.Text.Json;
using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Question> b)
    {
        b.ToTable("questions");
        b.HasKey(q => q.Id);

        b.Property(q => q.Id).HasColumnName("id");
        b.Property(q => q.LessonId).HasColumnName("lesson_id").IsRequired();
        b.Property(q => q.Type).HasColumnName("type")
            .HasConversion<string>().HasMaxLength(50).IsRequired();
        b.Property(q => q.Prompt).HasColumnName("prompt").HasMaxLength(2000).IsRequired();
        b.Property(q => q.CorrectAnswers)
            .HasColumnName("correct_answers")
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, _json),
                v => JsonSerializer.Deserialize<List<string>>(v, _json) ?? new())
            .IsRequired();
        b.Property(q => q.Options).HasColumnName("options").HasColumnType("json");
        b.Property(q => q.MediaUrl).HasColumnName("media_url").HasMaxLength(500);
        b.Property(q => q.OrderIndex).HasColumnName("order_index").IsRequired();
        b.Property(q => q.CreatedAt).HasColumnName("created_at");
        b.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(q => q.Lesson)
         .WithMany(l => l.Questions)
         .HasForeignKey(q => q.LessonId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(q => new { q.LessonId, q.OrderIndex });
    }
}
