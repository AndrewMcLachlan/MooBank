namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class ImportAccountConfiguration : IEntityTypeConfiguration<Domain.Entities.Account.ImportAccount>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.ImportAccount> entity)
    {
        entity.HasKey(e => e.AccountId);
        entity.HasOne(e => e.ImporterType).WithMany().HasForeignKey(i => i.ImporterTypeId);
    }
}
