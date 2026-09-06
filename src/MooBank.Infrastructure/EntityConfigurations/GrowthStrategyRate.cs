using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class GrowthStrategyRateConfiguration : IEntityTypeConfiguration<GrowthStrategyRate>
{
    public void Configure(EntityTypeBuilder<GrowthStrategyRate> builder)
    {
        builder.ToTable("GrowthStrategy");
    }
}
