using System.Text.Json;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> entity)
    {
        // Required do to computed column savings issues. See https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/breaking-changes#sqlserver-tables-with-triggers
        entity.ToTable(t => t.HasTrigger("FakeTrigger"));

        entity.HasKey("TransactionId");

        entity.Property(e => e.TransactionId).ValueGeneratedOnAdd();

        entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

        entity.Property(e => e.Description)
            .HasMaxLength(255)
            .IsUnicode(false);

        entity.Property(e => e.Amount).HasPrecision(10, 2);

        entity.Property(e => e.NetAmount).HasComputedColumnSql();

        entity.Property(e => e.TransactionTime).HasDefaultValueSql("(sysdatetime())");

        entity.HasOne(d => d.Account)
            .WithMany(p => p.Transactions)
            .HasForeignKey(d => d.AccountId);

        entity.HasMany(p => p.Splits)
            .WithOne(s => s.Transaction).HasForeignKey(e => e.TransactionId);

        entity.Property(e => e.TransactionType)
            .HasColumnName($"{nameof(Transaction.TransactionType)}Id")
            .HasConversion(e => (int)e, e => (Models.TransactionType)e)
            .HasDefaultValue(Models.TransactionType.Debit);

        entity.HasOne(e => e.AccountHolder).WithMany().HasForeignKey(e => e.AccountHolderId);

        //entity.HasOne(e => e.OffsetBy).WithOne(e => e.Offsets).HasForeignKey<Transaction>(t => t.OffsetByTransactionId);

        // This transaction is offset by the linked "OffsetTransactionId" transaction
        //entity.HasMany(e => e.OffsetBy).WithOne(e => e.TransactionSplit).HasForeignKey(t => t.TransactionSplitId);

        // This transaction offsets the linked "TransactionId" transaction
        entity.HasMany(e => e.OffsetFor).WithOne(e => e.OffsetByTransaction).HasForeignKey(t => t.OffsetTransactionId);


        entity.Property(e => e.Extra).HasConversion(
                       v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                       v => JsonSerializer.Deserialize<object>(v, JsonSerializerOptions.Default));
    }
}
