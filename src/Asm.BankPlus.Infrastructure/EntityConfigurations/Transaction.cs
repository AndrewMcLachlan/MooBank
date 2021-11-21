namespace Asm.BankPlus.Infrastructure.EntityConfigurations;

public class Transaction : IEntityTypeConfiguration<Data.Entities.Transaction>
{
    public void Configure(EntityTypeBuilder<Data.Entities.Transaction> entity)
    {
        entity.HasKey("TransactionId");

        entity.Property(e => e.TransactionId).ValueGeneratedOnAdd();

        entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

        entity.Property(e => e.Description)
            .HasMaxLength(255)
            .IsUnicode(false);

        entity.Property(e => e.TransactionTime).HasDefaultValueSql("(sysdatetime())");

        entity.HasOne(d => d.Account)
            .WithMany(p => p.Transactions)
            .HasForeignKey(d => d.AccountId);

        entity.HasMany(p => p.TransactionTags)
            .WithMany(t => t.Transactions)
            .UsingEntity<TransactionTransactionTag>(
                ttt => ttt.HasOne(ttt2 => ttt2.TransactionTag)
                          .WithMany()
                          .HasForeignKey(ttt2 => ttt2.TransactionTagId),
                ttt => ttt.HasOne(ttt2 => ttt2.Transaction)
                          .WithMany()
                          .HasForeignKey(ttt2 => ttt2.TransactionId),
                ttt =>
                {
                    ttt.HasKey(e => new { e.TransactionId, e.TransactionTagId });
                });

        entity.Property(e => e.TransactionType)
            .HasColumnName($"{nameof(Data.Entities.Transaction.TransactionType)}Id")
            .HasConversion(e => (int)e, e => (Models.TransactionType)e)
            .HasDefaultValue(Models.TransactionType.Debit);

    }
}
