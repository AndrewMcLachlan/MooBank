using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class InstitutionAccount : IEntityTypeConfiguration<Domain.Entities.Account.InstitutionAccount>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.InstitutionAccount> entity)
    {
        entity.Property(e => e.LastUpdated)
            .HasColumnType("datetimeoffset(0)")
            .HasDefaultValueSql("(sysutcdatetime())");

        entity.Property(r => r.AccountType)
            .HasConversion(e => (int)e, e => (Models.AccountType)e);

        entity.Property(r => r.AccountController)
            .HasConversion(e => (int)e, e => (Models.AccountController)e);

        entity.HasOne(e => e.ImportAccount)
              .WithOne()
            .HasForeignKey<Domain.Entities.Account.ImportAccount>(e => e.AccountId);

        entity.HasMany(e => e.VirtualAccounts)
              .WithOne(e => e.InstitutionAccount)
              .HasForeignKey(e => e.InstitutionAccountId);

    }
}
