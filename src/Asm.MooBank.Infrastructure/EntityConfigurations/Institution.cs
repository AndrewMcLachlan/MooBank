namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class Institution : IEntityTypeConfiguration<Domain.Entities.Institution.Institution>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Institution.Institution> entity)
    {
        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.InstitutionType).WithMany().HasForeignKey(e => e.InstitutionTypeId);
    }
}
