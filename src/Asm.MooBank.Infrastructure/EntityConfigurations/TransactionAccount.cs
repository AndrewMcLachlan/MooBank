using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionAccountConfiguration : IEntityTypeConfiguration<TransactionInstrument>
{
    public void Configure(EntityTypeBuilder<TransactionInstrument> entity)
    {
        // Required for computed columns
        entity.ToTable(t => t.HasTrigger("ComputedColumns"));

        entity.Property(e => e.CalculatedBalance)
            .ValueGeneratedOnAddOrUpdate();

        entity.Property(e => e.LastTransaction)
            .ValueGeneratedOnAddOrUpdate();
    }
}
