using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> b)
    {
        b.ToTable("question_options");
        b.HasKey(o => o.Id);

        b.Property(o => o.Id).HasColumnName("id");
        b.Property(o => o.QuestionId).HasColumnName("question_id").IsRequired();
        b.Property(o => o.OptionText).HasColumnName("option_text").HasMaxLength(1000).IsRequired();
        b.Property(o => o.OptionImageUrl).HasColumnName("option_image_url").HasMaxLength(500);
        b.Property(o => o.OptionAudioUrl).HasColumnName("option_audio_url").HasMaxLength(500);
        b.Property(o => o.IsCorrect).HasColumnName("is_correct").HasDefaultValue(false);
        b.Property(o => o.OrderIndex).HasColumnName("order_index").IsRequired();
        b.Property(o => o.CreatedAt).HasColumnName("created_at");
        b.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(o => o.QuestionId);

        b.HasOne(o => o.Question)
         .WithMany(q => q.Options)
         .HasForeignKey(o => o.QuestionId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
