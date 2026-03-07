using Asm.MooBank.Domain.Entities.Asset;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.Property(e => e.Id).HasColumnName("InstrumentId");
    }
}
