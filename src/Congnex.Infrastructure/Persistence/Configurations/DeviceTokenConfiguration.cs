using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> b)
    {
        b.ToTable("device_tokens");
        b.HasKey(t => t.Id);

        b.Property(t => t.Id).HasColumnName("id");
        b.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        b.Property(t => t.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        b.Property(t => t.Platform).HasColumnName("platform")
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(t => t.HubRegistrationId).HasColumnName("hub_registration_id").HasMaxLength(500);
        b.Property(t => t.CreatedAt).HasColumnName("created_at");
        b.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(t => t.User)
         .WithMany(u => u.DeviceTokens)
         .HasForeignKey(t => t.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        // One device token row per physical device token value
        b.HasIndex(t => t.Token).IsUnique();
        b.HasIndex(t => t.UserId);
    }
}
