namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class ImportAccount : IEntityTypeConfiguration<Data.Entities.ImportAccount>
{
    public void Configure(EntityTypeBuilder<Data.Entities.ImportAccount> entity)
    {
        entity.HasKey(e => e.AccountId);
        entity.HasOne(e => e.Account).WithOne(e => e.ImportAccount).HasForeignKey<Data.Entities.ImportAccount>(a => a.AccountId);
        entity.HasOne(e => e.ImporterType).WithMany().HasForeignKey(i => i.ImporterTypeId);
    }
}
