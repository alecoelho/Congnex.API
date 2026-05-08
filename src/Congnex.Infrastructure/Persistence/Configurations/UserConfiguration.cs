using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasColumnName("id");
        b.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        b.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        b.Ignore(u => u.FullName);
        b.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.PasswordHash).HasColumnName("password_hash");
        b.Property(u => u.GoogleSub).HasColumnName("google_sub");
        b.HasIndex(u => u.GoogleSub).IsUnique();
        b.Property(u => u.AppleSub).HasColumnName("apple_sub");
        b.HasIndex(u => u.AppleSub).IsUnique();
        b.Property(u => u.Xp).HasColumnName("xp").HasDefaultValue(0);
        b.Property(u => u.Streak).HasColumnName("streak").HasDefaultValue(0);
        b.Property(u => u.Lives).HasColumnName("lives").HasDefaultValue(3);
        b.Property(u => u.Energy).HasColumnName("energy").HasDefaultValue(100);
        b.Property(u => u.MaxEnergy).HasColumnName("max_energy").HasDefaultValue(100);
        b.Property(u => u.LastLessonAt).HasColumnName("last_lesson_at");
        b.Property(u => u.LastLifeRegenAt).HasColumnName("last_life_regen_at");
        b.Property(u => u.Plan).HasColumnName("plan")
            .HasConversion<string>().HasDefaultValue(UserPlan.Free);
        b.Property(u => u.DailyMinutes).HasColumnName("daily_minutes").HasDefaultValue(10);
        b.Property(u => u.Language).HasColumnName("language").HasMaxLength(10).HasDefaultValue("en");
        b.Property(u => u.Motivations).HasColumnName("motivations").HasColumnType("json");
        b.Property(u => u.RefreshTokenHash).HasColumnName("refresh_token_hash");
        b.Property(u => u.RefreshTokenExpiresAt).HasColumnName("refresh_token_expires_at");
        b.Property(u => u.CreatedAt).HasColumnName("created_at");
        b.Property(u => u.UpdatedAt).HasColumnName("updated_at");
    }
}
