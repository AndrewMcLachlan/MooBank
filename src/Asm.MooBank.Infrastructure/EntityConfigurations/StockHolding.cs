using Asm.MooBank.Domain.Entities.StockHolding;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class StockHoldingConfiguration : IEntityTypeConfiguration<StockHolding>
{
    public void Configure(EntityTypeBuilder<StockHolding> builder)
    {
        builder.UseTptMappingStrategy();

        builder.ToTable(t => t.HasTrigger("ComputedColumns"));

        builder.OwnsOne(e => e.Symbol,
            b =>
            {
                b.Property(e => e.Symbol).HasColumnName("Symbol").HasMaxLength(3);
                b.Property(e => e.Exchange).HasColumnName("Exchange").HasMaxLength(2);
            });

        builder.HasMany(e => e.Transactions).WithOne(e => e.StockHolding)
               .HasForeignKey(e => e.AccountId);

        builder.Property(e => e.Quantity)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.GainLoss)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.CurrentValue)
            .ValueGeneratedOnAddOrUpdate();

    }
}
