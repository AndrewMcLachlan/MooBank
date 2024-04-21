using Asm.MooBank.Domain.Entities.Asset;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.UseTptMappingStrategy();

        //builder.ToTable(t => t.HasTrigger("ComputedColumns"));

        builder.Property(e => e.PurchasePrice)
            .HasColumnType("decimal(12, 4)");

    }
}
