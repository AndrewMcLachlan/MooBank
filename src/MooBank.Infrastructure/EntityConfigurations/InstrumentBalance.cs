using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class InstrumentBalanceConfiguration : IEntityTypeConfiguration<InstrumentBalance>
{
    public void Configure(EntityTypeBuilder<InstrumentBalance> builder)
    {
        builder.HasKey(e => e.InstrumentId);

        // View-only read model. OnModelCreating assigns every entity a table name by convention;
        // ToTable(null) clears it so this type has a single table base (the view) and is never
        // treated as writable.
        builder.ToView("TransactionInstrumentBalance");
        builder.ToTable((string?)null);

        builder.Property(e => e.Balance).HasPrecision(12, 4);
    }
}
