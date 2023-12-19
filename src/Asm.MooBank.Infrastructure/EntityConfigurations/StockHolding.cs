using Asm.MooBank.Domain.Entities.StockHolding;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class StockHoldingConfiguration : IEntityTypeConfiguration<StockHolding>
{
    public void Configure(EntityTypeBuilder<StockHolding> builder)
    {
        builder.UseTptMappingStrategy();

        builder.ToTable(t => t.HasTrigger("ComputedColumns"));

        builder.Property(e => e.Symbol).HasMaxLength(3);
        builder.Property(e => e.Exchange).HasMaxLength(2);

        builder.HasMany(e => e.Transactions).WithOne(e => e.StockHolding)
               .HasForeignKey(e => e.AccountId);

        builder.Property(e => e.Quantity)
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.CurrentPrice).HasColumnType("decimal(10, 2)");

        builder.Property(e => e.CurrentValue)
            .HasColumnType("decimal(10, 2)")
            .ValueGeneratedOnAddOrUpdate();
    }
}
