using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class PensionRateConfiguration : IEntityTypeConfiguration<PensionRate>
{
    public void Configure(EntityTypeBuilder<PensionRate> builder)
    {
        builder.ToTable("PensionRate");
    }
}
