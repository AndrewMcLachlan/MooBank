using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionOffsetConfiguration : IEntityTypeConfiguration<TransactionOffset>
{
    public void Configure(EntityTypeBuilder<TransactionOffset> entity)
    {
        entity.HasKey(e => new { e.TransactionId, e.OffsetTransactionId })
              .HasName("PK_TransactionOffset");
    }
}
