using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> b)
    {
        b.ToTable("notification_preferences");
        b.HasKey(p => p.Id);

        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        b.Property(p => p.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(true);
        b.Property(p => p.FrequencyHours).HasColumnName("frequency_hours").HasDefaultValue(2);
        b.Property(p => p.StartHour).HasColumnName("start_hour").HasDefaultValue(8);
        b.Property(p => p.EndHour).HasColumnName("end_hour").HasDefaultValue(20);
        b.Property(p => p.ContentTypes).HasColumnName("content_types").HasColumnType("json").IsRequired();
        b.Property(p => p.CreatedAt).HasColumnName("created_at");
        b.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(p => p.User)
         .WithMany()
         .HasForeignKey(p => p.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        // One preference row per user
        b.HasIndex(p => p.UserId).IsUnique();
    }
}
