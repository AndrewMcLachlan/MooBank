using System.Reflection.Metadata;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionSplitConfiguration : IEntityTypeConfiguration<TransactionSplit>
{
    public void Configure(EntityTypeBuilder<TransactionSplit> entity)
    {
        // Required do to computed column savings issues. See https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/breaking-changes#sqlserver-tables-with-triggers
        entity.ToTable(tb => tb.UseSqlOutputClause(false));

        entity.HasMany(p => p.Tags)
            .WithMany()
            .UsingEntity<TransactionSplitTag>(
        tst => tst.HasOne(tst2 => tst2.Tag)
                  .WithMany()
                  .HasForeignKey(tst2 => tst2.TagId),
        tst => tst.HasOne(tst2 => tst2.TransactionSplit)
                  .WithMany()
                  .HasForeignKey(tst2 => tst2.TransactionSplitId),
        tst =>
        {
            tst.HasKey(e => new { e.TransactionSplitId, e.TagId });
        });


    }
}
