using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("units");
        b.HasKey(u => u.Id);

        b.Property(u => u.Id).HasColumnName("id");
        b.Property(u => u.LanguageCode).HasColumnName("language_code").HasMaxLength(10).IsRequired();
        b.Property(u => u.OrderIndex).HasColumnName("order_index").IsRequired();
        b.Property(u => u.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        b.Property(u => u.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
        b.Property(u => u.CreatedAt).HasColumnName("created_at");
        b.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(u => new { u.LanguageCode, u.OrderIndex }).IsUnique();
    }
}
