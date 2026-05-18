using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class VideoLearningItemConfiguration : IEntityTypeConfiguration<VideoLearningItem>
{
    public void Configure(EntityTypeBuilder<VideoLearningItem> b)
    {
        b.ToTable("video_learning_items");
        b.HasKey(i => i.Id);

        b.Property(i => i.Id).HasColumnName("id");
        b.Property(i => i.VideoId).HasColumnName("video_id").IsRequired();
        b.Property(i => i.ItemType).HasColumnName("item_type").HasMaxLength(50).IsRequired();
        b.Property(i => i.TextEn).HasColumnName("text_en").HasMaxLength(1000).IsRequired();
        b.Property(i => i.TextPt).HasColumnName("text_pt").HasMaxLength(1000);
        b.Property(i => i.Category).HasColumnName("category").HasMaxLength(100);
        b.Property(i => i.Difficulty).HasColumnName("difficulty").HasMaxLength(20).HasDefaultValue("easy");
        b.Property(i => i.TimestampStart).HasColumnName("timestamp_start");
        b.Property(i => i.TimestampEnd).HasColumnName("timestamp_end");
        b.Property(i => i.CreatedAt).HasColumnName("created_at");
        b.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(i => i.VideoId);

        b.HasOne(i => i.Video)
         .WithMany(v => v.LearningItems)
         .HasForeignKey(i => i.VideoId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
