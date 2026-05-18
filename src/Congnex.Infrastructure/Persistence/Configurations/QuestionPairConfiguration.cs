using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class QuestionPairConfiguration : IEntityTypeConfiguration<QuestionPair>
{
    public void Configure(EntityTypeBuilder<QuestionPair> b)
    {
        b.ToTable("question_pairs");
        b.HasKey(p => p.Id);

        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.QuestionId).HasColumnName("question_id").IsRequired();
        b.Property(p => p.LeftText).HasColumnName("left_text").HasMaxLength(1000).IsRequired();
        b.Property(p => p.RightText).HasColumnName("right_text").HasMaxLength(1000).IsRequired();
        b.Property(p => p.LeftAudioUrl).HasColumnName("left_audio_url").HasMaxLength(500);
        b.Property(p => p.RightAudioUrl).HasColumnName("right_audio_url").HasMaxLength(500);
        b.Property(p => p.LeftImageUrl).HasColumnName("left_image_url").HasMaxLength(500);
        b.Property(p => p.RightImageUrl).HasColumnName("right_image_url").HasMaxLength(500);
        b.Property(p => p.OrderIndex).HasColumnName("order_index").IsRequired();
        b.Property(p => p.CreatedAt).HasColumnName("created_at");
        b.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(p => p.QuestionId);

        b.HasOne(p => p.Question)
         .WithMany(q => q.Pairs)
         .HasForeignKey(p => p.QuestionId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
