namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class VirtualAccount : IEntityTypeConfiguration<Domain.Entities.Account.VirtualAccount>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.VirtualAccount> entity)
    {
        entity.Property(e => e.Id).HasColumnName("AccountId");

        //entity.HasOne(e => e.InstitutionAccount).WithMany(e => e.VirtualAccounts).HasForeignKey(e => e.InstitutionAccountId);
    }
}
