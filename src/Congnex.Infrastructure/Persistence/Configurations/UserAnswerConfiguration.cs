using System.Text.Json;
using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class UserAnswerConfiguration : IEntityTypeConfiguration<UserAnswer>
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<UserAnswer> b)
    {
        b.ToTable("user_answers");
        b.HasKey(a => a.Id);

        b.Property(a => a.Id).HasColumnName("id");
        b.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        b.Property(a => a.QuestionId).HasColumnName("question_id").IsRequired();
        b.Property(a => a.GivenAnswers)
            .HasColumnName("given_answers")
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, _json),
                v => JsonSerializer.Deserialize<List<string>>(v, _json) ?? new())
            .IsRequired();
        b.Property(a => a.IsCorrect).HasColumnName("is_correct").IsRequired();
        b.Property(a => a.AnsweredAt).HasColumnName("answered_at").IsRequired();
        b.Property(a => a.CreatedAt).HasColumnName("created_at");
        b.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(a => a.User)
         .WithMany()
         .HasForeignKey(a => a.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(a => a.Question)
         .WithMany(q => q.Answers)
         .HasForeignKey(a => a.QuestionId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(a => new { a.UserId, a.AnsweredAt });
    }
}
