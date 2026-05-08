using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class StudyPlanConfiguration : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> b)
    {
        b.ToTable("study_plans");
        b.HasKey(p => p.Id);

        b.Property(p => p.Id).HasColumnName("id");
        b.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        b.Property(p => p.AssessedLevel).HasColumnName("assessed_level").HasMaxLength(50).IsRequired();
        b.Property(p => p.Content).HasColumnName("content").HasColumnType("json").IsRequired();
        b.Property(p => p.GeneratedAt).HasColumnName("generated_at").IsRequired();
        b.Property(p => p.CreatedAt).HasColumnName("created_at");
        b.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(p => p.User)
         .WithMany()
         .HasForeignKey(p => p.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(p => new { p.UserId, p.GeneratedAt });
    }
}
