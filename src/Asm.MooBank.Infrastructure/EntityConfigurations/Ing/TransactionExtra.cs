namespace Asm.MooBank.Infrastructure.EntityConfigurations.Ing;

public class TransactionExtra : IEntityTypeConfiguration<Domain.Entities.Ing.TransactionExtra>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Ing.TransactionExtra> entity)
    {
        entity.ToTable("TransactionExtra", "ing");

        entity.HasKey(e => e.TransactionId);

        entity.HasOne(e => e.Transaction).WithOne().HasForeignKey<Domain.Entities.Ing.TransactionExtra>(e => e.TransactionId);
    }
}
