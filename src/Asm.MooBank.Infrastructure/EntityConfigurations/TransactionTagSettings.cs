namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TransactionTagSettingsConfiguration : IEntityTypeConfiguration<Domain.Entities.Tag.TransactionTagSettings>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Tag.TransactionTagSettings> entity)
    {
        entity.ToTable("TransactionTagSettings");

        //entity.HasKey("TransactionTagId");
        entity.HasKey(e => e.TransactionTagId);
    }
}