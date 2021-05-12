using System;//
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asm.BankPlus.Data.EntityConfigurations
{
    public class Account : IEntityTypeConfiguration<Entities.Account>
    {
        public void Configure(EntityTypeBuilder<Entities.Account> entity)
        {
            entity.HasKey(e => e.AccountId);

            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.AccountId).ValueGeneratedOnAdd();
            entity.Property(e => e.AccountId).HasDefaultValueSql("(newid())");

            entity.Property(e => e.AccountBalance).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.AvailableBalance).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Description).HasMaxLength(255);

            entity.Property(e => e.LastUpdated)
                .HasColumnType("datetimeoffset(0)")
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(r => r.AccountType)
            .HasConversion(e => (int)e, e => (Models.AccountType)e);

            entity.Property(r => r.AccountController)
            .HasConversion(e => (int)e, e => (Models.AccountController)e);

            entity.HasMany(p => p.AccountHolders)
                  .WithMany(t => t.Accounts)
                  .UsingEntity<AccountAccountHolder>(
                       aah => aah.HasOne(aah2 => aah2.AccountHolder)
                                 .WithMany()
                                 .HasForeignKey(aah2 => aah2.AccountHolderId),
                       aah => aah.HasOne(aah2 => aah2.Account)
                                 .WithMany()
                                  .HasForeignKey(aah2 => aah2.AccountId),
                       aah =>
                       {
                           aah.HasKey(e => new { e.AccountId, e.AccountHolderId });
                       });

            entity.HasOne(e => e.ImportAccount)
                  .WithOne(e => e.Account)
                  .HasForeignKey<Entities.Account>(e => e.AccountId);

        }
    }
}
