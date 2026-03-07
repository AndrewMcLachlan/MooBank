using System.Text.Json;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> entity)
    {
        // Required do to computed column savings issues. See https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/breaking-changes#sqlserver-tables-with-triggers
        entity.ToTable(tb => tb.UseSqlOutputClause(false));

        //entity.HasKey("Id");
        entity.Property(t => t.Id).HasColumnName("TransactionId");

        entity.HasOne(d => d.Account)
            .WithMany(p => p.Transactions)
            .HasForeignKey(d => d.AccountId);

        entity.HasMany(p => p.Splits)
            .WithOne(s => s.Transaction).HasForeignKey(e => e.TransactionId);

        entity.Property(e => e.TransactionType)
            .HasColumnName($"{nameof(Transaction.TransactionType)}Id")
            .HasConversion(e => (int)e, e => (Models.TransactionType)e)
            .HasDefaultValue(Models.TransactionType.Debit)
            .HasSentinel(Models.TransactionType.NotSet);

        entity.Property(e => e.TransactionSubType)
            .HasColumnName($"{nameof(Transaction.TransactionSubType)}Id")
            .HasConversion(e => (int?)e, e => (Models.TransactionSubType?)e)
            .HasSentinel(null);

        // This transaction offsets the linked "TransactionId" transaction
        entity.HasMany(e => e.OffsetFor).WithOne(e => e.OffsetByTransaction).HasForeignKey(t => t.OffsetTransactionId);

        entity.Property(e => e.Extra).HasConversion(
                       v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                       v => JsonSerializer.Deserialize<object>(v, JsonSerializerOptions.Default));
    }
}
