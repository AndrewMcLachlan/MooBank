using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Institution.Ing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asm.MooBank.Institution.Ing.Infrastructure;

internal class TransactionRawEntityConfiguration : IEntityTypeConfiguration<TransactionRaw>
{
    public void Configure(EntityTypeBuilder<TransactionRaw> entity)
    {
        entity.ToTable("TransactionRaw", "ing");

        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.Transaction).WithOne().HasForeignKey<TransactionRaw>(e => e.TransactionId);
    }
}
