namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class TransactionAccountConfiguration : IEntityTypeConfiguration<Domain.Entities.Account.TransactionAccount>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.TransactionAccount> entity)
    {
        // Required for computed columns
        entity.ToTable(t => t.HasTrigger("ComputedColumns"));

        entity.Property(e => e.CalculatedBalance)
            .ValueGeneratedOnAddOrUpdate();

        entity.Property(e => e.LastTransaction)
            .ValueGeneratedOnAddOrUpdate();
    }
}
