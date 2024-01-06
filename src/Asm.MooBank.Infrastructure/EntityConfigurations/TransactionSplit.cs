using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionSplitConfiguration : IEntityTypeConfiguration<TransactionSplit>
{
    public void Configure(EntityTypeBuilder<TransactionSplit> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.Property(e => e.Amount).HasPrecision(10, 2);

        entity.Property(e => e.NetAmount).HasComputedColumnSql();

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
