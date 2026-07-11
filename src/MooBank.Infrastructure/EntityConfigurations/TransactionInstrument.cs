using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionInstrumentConfiguration : IEntityTypeConfiguration<TransactionInstrument>
{
    public void Configure(EntityTypeBuilder<TransactionInstrument> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false)).Property(e => e.Id).HasColumnName("InstrumentId");
        builder.Property(e => e.Id).HasColumnName("InstrumentId");

        // Balance and LastTransaction are read from the TransactionInstrumentBalance view via a
        // 1:1 read-only navigation. It is auto-included so every instrument load carries its
        // derived balance without callers needing an explicit Include. The view is joined (not a
        // second table base on this TPT type), which is why this works where a direct table+view
        // mapping on TransactionInstrument does not.
        builder.HasOne(e => e.BalanceInfo)
            .WithOne()
            .HasForeignKey<InstrumentBalance>(e => e.InstrumentId);

        builder.Navigation(e => e.BalanceInfo).AutoInclude();
    }
}
