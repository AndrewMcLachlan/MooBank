using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.Property(e => e.TransactionType)
            .HasColumnName($"{nameof(Transaction.TransactionType)}Id")
            .HasConversion(e => (int)e, e => (Models.TransactionType)e)
            .HasDefaultValue(Models.TransactionType.Debit)
            .HasSentinel(Models.TransactionType.NotSet);
    }
}
