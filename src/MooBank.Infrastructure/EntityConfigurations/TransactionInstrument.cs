using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionInstrumentConfiguration : IEntityTypeConfiguration<TransactionInstrument>
{
    public void Configure(EntityTypeBuilder<TransactionInstrument> builder)
    {
        // Writes target the TransactionInstrument table; reads come from the
        // TransactionInstrumentBalance view, which supplies the derived Balance and
        // LastTransaction (see dbo.Views.TransactionInstrumentBalance). Those two
        // properties are DatabaseGeneratedOption.Computed, so EF never writes them to
        // the table — it only reads them from the view.
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
        builder.ToView("TransactionInstrumentBalance");
        builder.Property(e => e.Id).HasColumnName("InstrumentId");
    }
}
