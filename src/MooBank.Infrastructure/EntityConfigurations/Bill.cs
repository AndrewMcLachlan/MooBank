using Asm.MooBank.Domain.Entities.Utility;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));

        builder
            .HasMany(b => b.Discounts)
            .WithMany(d => d.Bills)
            .UsingEntity<Dictionary<string, object>>(
                "DiscountBill",
                j => j.HasOne<Discount>().WithMany().HasForeignKey("DiscountId"),
                j => j.HasOne<Bill>().WithMany().HasForeignKey("BillId")
            );
    }
}
