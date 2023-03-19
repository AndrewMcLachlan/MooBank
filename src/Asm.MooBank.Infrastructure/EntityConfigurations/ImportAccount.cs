namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class ImportAccount : IEntityTypeConfiguration<Domain.Entities.Account.ImportAccount>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.ImportAccount> entity)
    {
        entity.HasKey(e => e.AccountId);
        //entity.HasOne(e => e.InstitutionAccount).WithOne(e => e.ImportAccount).HasForeignKey<Domain.Entities.Account.ImportAccount>(a => a.AccountId);
        entity.HasOne(e => e.ImporterType).WithMany().HasForeignKey(i => i.ImporterTypeId);
    }
}
