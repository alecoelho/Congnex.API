using Congnex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Congnex.Infrastructure.Persistence.Configurations;

public class QuestionBankOptionConfiguration : IEntityTypeConfiguration<QuestionBankOption>
{
    public void Configure(EntityTypeBuilder<QuestionBankOption> b)
    {
        b.ToTable("question_bank_options");
        b.HasKey(o => o.Id);

        b.Property(o => o.Id).HasColumnName("id");
        b.Property(o => o.QuestionBankId).HasColumnName("question_bank_id").IsRequired();
        b.Property(o => o.OptionText).HasColumnName("option_text").HasMaxLength(500).IsRequired();
        b.Property(o => o.IsCorrect).HasColumnName("is_correct");
        b.Property(o => o.OrderIndex).HasColumnName("order_index");
        b.Property(o => o.CreatedAt).HasColumnName("created_at");
        b.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(o => o.Question)
         .WithMany(q => q.Options)
         .HasForeignKey(o => o.QuestionBankId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
