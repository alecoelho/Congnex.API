using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class AiQuestionConfiguration : IEntityTypeConfiguration<AiQuestion>
{
    public void Configure(EntityTypeBuilder<AiQuestion> b)
    {
        b.ToTable("ai_questions");
        b.HasKey(q => q.Id);

        b.Property(q => q.Id).HasColumnName("id");
        b.Property(q => q.UserId).HasColumnName("user_id").IsRequired();
        b.Property(q => q.Prompt).HasColumnName("prompt").HasMaxLength(2000).IsRequired();
        b.Property(q => q.Options).HasColumnName("options").HasColumnType("json").IsRequired();
        b.Property(q => q.CorrectIndex).HasColumnName("correct_index").IsRequired();
        b.Property(q => q.Topic).HasColumnName("topic").HasMaxLength(200).IsRequired();
        b.Property(q => q.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
        b.Property(q => q.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        b.Property(q => q.CreatedAt).HasColumnName("created_at");
        b.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(q => q.User)
         .WithMany()
         .HasForeignKey(q => q.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(q => new { q.UserId, q.IsActive });
    }
}
