using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class UserInterviewAnswerConfiguration : IEntityTypeConfiguration<UserInterviewAnswer>
{
    public void Configure(EntityTypeBuilder<UserInterviewAnswer> b)
    {
        b.ToTable("user_interview_answers");
        b.HasKey(a => a.Id);

        b.Property(a => a.Id).HasColumnName("id");
        b.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        b.Property(a => a.EnglishLevel).HasColumnName("english_level").HasMaxLength(5).IsRequired();
        b.Property(a => a.VideoUrl).HasColumnName("video_url").HasMaxLength(1000).IsRequired();
        b.Property(a => a.VideoCategory).HasColumnName("video_category").HasMaxLength(100).IsRequired();
        b.Property(a => a.CreatedAt).HasColumnName("created_at");
        b.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(a => a.User)
         .WithMany()
         .HasForeignKey(a => a.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(a => a.UserId);
    }
}
