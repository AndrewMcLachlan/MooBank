namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class TagSettingsConfiguration : IEntityTypeConfiguration<Domain.Entities.Tag.TagSettings>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Tag.TagSettings> entity)
    {
        entity.ToTable("TagSettings");

        entity.HasKey(e => e.TransactionTagId);
    }
}
