using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Institution.Macquarie.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asm.MooBank.Institution.Macquarie.Infrastructure;

internal class TransactionRawEntityConfiguration : IEntityTypeConfiguration<TransactionRaw>
{
    public void Configure(EntityTypeBuilder<TransactionRaw> entity)
    {
        entity.ToTable("TransactionRaw", "macquarie");

        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Transaction).WithOne().HasForeignKey<TransactionRaw>(e => e.TransactionId);
    }
}
