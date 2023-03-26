using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class Account : IEntityTypeConfiguration<Domain.Entities.Account.Account>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.Account> entity)
    {
        entity.HasKey(e => e.AccountId);

        entity.HasIndex(e => e.Name).IsUnique();

        entity.Property(e => e.AccountId).ValueGeneratedOnAdd();
        entity.Property(e => e.AccountId).HasDefaultValueSql("(newid())");

        entity.Property(e => e.Balance).HasColumnType("decimal(10, 2)");

        entity.Property(e => e.Description).HasMaxLength(255);

        entity.Property(e => e.LastUpdated)
            .HasColumnType("datetimeoffset(0)")
            .HasDefaultValueSql("(sysutcdatetime())");

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        /*entity.HasMany(p => p.AccountAccountHolders)
               .WithOne(e => e.Account)
               .HasPrincipalKey(e => e.AccountId)
               .HasForeignKey(p => p.AccountId);*/
        /*
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

        entity.HasMany(p => p.AccountGroups)
              .WithMany(t => t.Accounts)
              .UsingEntity<AccountAccountHolder>(
                   aah => aah.HasOne(aah2 => aah2.AccountGroup)
                             .WithMany()
                             .HasForeignKey(aah2 => aah2.AccountGroupId),
                   aah => aah.HasOne(aah2 => aah2.Account)
                             .WithMany()
                              .HasForeignKey(aah2 => aah2.AccountId),
                   aah =>
                   {
                       aah.HasKey(e => new { e.AccountId, e.AccountHolderId });
                   });
        */
    }
}
