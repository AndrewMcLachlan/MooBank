using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;
internal class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Price).HasColumnType("decimal(12, 4)");

        builder.Property(e => e.Fees).HasColumnType("decimal(12, 4)");

        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.AccountHolderId);

        builder.Property(e => e.TransactionType)
            .HasColumnName($"{nameof(Transaction.TransactionType)}Id")
            .HasConversion(e => (int)e, e => (Models.TransactionType)e)
            .HasDefaultValue(Models.TransactionType.Debit);
    }
}
