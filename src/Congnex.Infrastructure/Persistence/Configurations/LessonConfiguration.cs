using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> b)
    {
        b.ToTable("lessons");
        b.HasKey(l => l.Id);

        b.Property(l => l.Id).HasColumnName("id");
        b.Property(l => l.UnitId).HasColumnName("unit_id").IsRequired();
        b.Property(l => l.UserId).HasColumnName("user_id");
        b.Property(l => l.OrderIndex).HasColumnName("order_index").IsRequired();
        b.Property(l => l.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        b.Property(l => l.Description).HasColumnName("description").HasMaxLength(1000);
        b.Property(l => l.XpReward).HasColumnName("xp_reward").HasDefaultValue(10);
        b.Property(l => l.Level).HasColumnName("level").HasMaxLength(5);
        b.Property(l => l.CreatedAt).HasColumnName("created_at");
        b.Property(l => l.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(l => l.Unit)
         .WithMany(u => u.Lessons)
         .HasForeignKey(l => l.UnitId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(l => l.User)
         .WithMany()
         .HasForeignKey(l => l.UserId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(l => new { l.UnitId, l.OrderIndex });
        b.HasIndex(l => l.UserId);
    }
}
