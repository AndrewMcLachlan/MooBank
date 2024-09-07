using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;
internal class StockPriceHistoryConfiguration : IEntityTypeConfiguration<StockPriceHistory>
{
    public void Configure(EntityTypeBuilder<StockPriceHistory> builder)
    {
        /*builder.OwnsOne(e => e.Symbol,
            b =>
            {
                b.Property(e => e.Symbol).HasColumnName("Symbol").HasMaxLength(3);
                b.Property(e => e.Exchange).HasColumnName("Exchange").HasMaxLength(2);
            });*/
    }
}
