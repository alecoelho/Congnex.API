using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class QuestionBankConfiguration : IEntityTypeConfiguration<QuestionBank>
{
    public void Configure(EntityTypeBuilder<QuestionBank> b)
    {
        b.ToTable("question_bank");
        b.HasKey(q => q.Id);

        b.Property(q => q.Id).HasColumnName("id");
        b.Property(q => q.CefrLevel).HasColumnName("cefr_level").HasMaxLength(2).IsRequired();
        b.Property(q => q.QuestionType).HasColumnName("question_type").HasMaxLength(50).IsRequired();
        b.Property(q => q.Domain).HasColumnName("domain").HasMaxLength(50).IsRequired();
        b.Property(q => q.Type).HasColumnName("type").HasMaxLength(30).IsRequired();
        b.Property(q => q.QuestionText).HasColumnName("question_text").HasMaxLength(1000).IsRequired();
        b.Property(q => q.CorrectAnswer).HasColumnName("correct_answer").HasMaxLength(500).IsRequired();
        b.Property(q => q.Difficulty).HasColumnName("difficulty").HasMaxLength(10).HasDefaultValue("easy");
        b.Property(q => q.CreatedAt).HasColumnName("created_at");
        b.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(q => new { q.CefrLevel, q.QuestionType, q.Domain });
        b.HasIndex(q => q.Difficulty);
    }
}
