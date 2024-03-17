using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Institution.AustralianSuper.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asm.MooBank.Institution.AustralianSuper.Infrastructure;

internal class TransactionRawEntityConfiguration : IEntityTypeConfiguration<TransactionRaw>
{
    public void Configure(EntityTypeBuilder<TransactionRaw> entity)
    {
        entity.ToTable("TransactionRaw", "aussuper");

        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Transaction).WithOne().HasForeignKey<TransactionRaw>(e => e.TransactionId);
    }
}
