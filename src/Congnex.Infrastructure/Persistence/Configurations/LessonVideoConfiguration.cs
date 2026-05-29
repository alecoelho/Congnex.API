using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class LessonVideoConfiguration : IEntityTypeConfiguration<LessonVideo>
{
    public void Configure(EntityTypeBuilder<LessonVideo> b)
    {
        b.ToTable("lesson_videos");
        b.HasKey(v => v.Id);

        b.Property(v => v.Id).HasColumnName("id");
        b.Property(v => v.LessonId).HasColumnName("lesson_id").IsRequired();
        b.Property(v => v.YoutubeVideoId).HasColumnName("youtube_video_id").HasMaxLength(50).IsRequired();
        b.Property(v => v.YoutubeUrl).HasColumnName("youtube_url").HasMaxLength(500).IsRequired();
        b.Property(v => v.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        b.Property(v => v.TranscriptJson).HasColumnName("transcript_json").HasColumnType("json");
        b.Property(v => v.Language).HasColumnName("language").HasMaxLength(10).HasDefaultValue("en");
        b.Property(v => v.DurationSeconds).HasColumnName("duration_seconds").HasDefaultValue(0);
        b.Property(v => v.StartTime).HasColumnName("start_time");
        b.Property(v => v.EndTime).HasColumnName("end_time");
        b.Property(v => v.CreatedAt).HasColumnName("created_at");
        b.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(v => v.LessonId);

        b.HasOne(v => v.Lesson)
         .WithMany(l => l.Videos)
         .HasForeignKey(v => v.LessonId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
