using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class MinimumDrawdownRateConfiguration : IEntityTypeConfiguration<MinimumDrawdownRate>
{
    public void Configure(EntityTypeBuilder<MinimumDrawdownRate> builder)
    {
        builder.ToTable("MinimumDrawdownRate");
    }
}
