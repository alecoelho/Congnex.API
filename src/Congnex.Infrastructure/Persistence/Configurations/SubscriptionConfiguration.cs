using Congnex.Domain.Entities;
using Congnex.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.ToTable("subscriptions");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).HasColumnName("id");
        b.Property(s => s.UserId).HasColumnName("user_id");
        b.Property(s => s.StripeCustomerId).HasColumnName("stripe_customer_id").HasMaxLength(100).IsRequired();
        b.Property(s => s.StripeSubscriptionId).HasColumnName("stripe_subscription_id").HasMaxLength(100);
        b.HasIndex(s => s.StripeSubscriptionId).IsUnique();
        b.HasIndex(s => s.StripeCustomerId);
        b.Property(s => s.StripePriceId).HasColumnName("stripe_price_id").HasMaxLength(100);
        b.Property(s => s.Status).HasColumnName("status")
            .HasConversion<string>();
        b.Property(s => s.CancelAtPeriodEnd).HasColumnName("cancel_at_period_end").HasDefaultValue(false);
        b.Property(s => s.CancelAt).HasColumnName("cancel_at");
        b.Property(s => s.CanceledAt).HasColumnName("canceled_at");
        b.Property(s => s.CurrentPeriodStart).HasColumnName("current_period_start");
        b.Property(s => s.CurrentPeriodEnd).HasColumnName("current_period_end");
        b.Property(s => s.CreatedAt).HasColumnName("created_at");
        b.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
