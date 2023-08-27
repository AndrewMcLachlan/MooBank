namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class Family : IEntityTypeConfiguration<Domain.Entities.Family.Family>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Family.Family> entity)
    {
        entity.HasKey(e => e.Id);
    }
}
