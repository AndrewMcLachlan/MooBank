using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionInstrumentConfiguration : IEntityTypeConfiguration<TransactionInstrument>
{
    public void Configure(EntityTypeBuilder<TransactionInstrument> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false)).Property(e => e.Id).HasColumnName("InstrumentId");
        builder.Property(e => e.Id).HasColumnName("InstrumentId");
    }
}
