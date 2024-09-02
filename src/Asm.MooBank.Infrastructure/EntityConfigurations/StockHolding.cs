using Asm.MooBank.Domain.Entities.StockHolding;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class StockHoldingConfiguration : IEntityTypeConfiguration<StockHolding>
{
    public void Configure(EntityTypeBuilder<StockHolding> builder)
    {
        builder.UseTptMappingStrategy();

        builder.ToTable(tb => tb.UseSqlOutputClause(false));

        builder.OwnsOne(e => e.Symbol,
            b =>
            {
                b.Property(e => e.Symbol).HasColumnName("Symbol").HasMaxLength(3);
                b.Property(e => e.Exchange).HasColumnName("Exchange").HasMaxLength(2);
            });

        builder.HasMany(e => e.Transactions).WithOne(e => e.StockHolding)
               .HasForeignKey(e => e.AccountId);

    }
}
