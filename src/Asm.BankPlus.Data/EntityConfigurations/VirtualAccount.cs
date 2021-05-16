using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asm.BankPlus.Data.EntityConfigurations
{
    public class VirtualAccount : IEntityTypeConfiguration<Entities.VirtualAccount>
    {
        public void Configure(EntityTypeBuilder<Entities.VirtualAccount> entity)
        {
            entity.Property(e => e.VirtualAccountId).HasDefaultValueSql("(newid())");

            entity.Property(e => e.Balance).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Description)
                  .HasMaxLength(255)
                  .IsUnicode(false);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50)
                  .IsUnicode(false);

            entity.HasOne(e => e.Account).WithMany(e => e.VirtualAccounts).HasForeignKey(e => e.AccountId);


        }
    }
}
