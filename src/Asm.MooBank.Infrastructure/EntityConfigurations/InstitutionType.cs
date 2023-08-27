namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class InstitutionType : IEntityTypeConfiguration<Domain.Entities.Institution.InstitutionType>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Institution.InstitutionType> entity)
    {
        entity.HasKey(e => e.Id);
    }
}
