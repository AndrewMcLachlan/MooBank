using Asm.BankPlus.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Asm.BankPlus.Data.EntityConfigurations
{
    public class ImportAccount : IEntityTypeConfiguration<Entities.ImportAccount>
    {
        public void Configure(EntityTypeBuilder<Entities.ImportAccount> entity)
        {
            entity.HasKey(e => e.AccountId);
            entity.HasOne(e => e.Account).WithOne(e => e.ImportAccount).HasForeignKey<Entities.ImportAccount>(a => a.AccountId);
            entity.HasOne(e => e.ImporterType).WithMany().HasForeignKey(i => i.ImporterTypeId);
        }
    }
}
