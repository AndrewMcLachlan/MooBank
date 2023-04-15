namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TransactionTagSettings : IEntityTypeConfiguration<Domain.Entities.TransactionTags.TransactionTagSettings>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TransactionTags.TransactionTagSettings> entity)
    {
        entity.ToTable("TransactionTagSettings");

        //entity.HasKey("TransactionTagId");
        entity.HasKey(e => e.TransactionTagId);
    }
}