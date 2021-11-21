namespace Asm.BankPlus.Infrastructure.EntityConfigurations;

public class Account : IEntityTypeConfiguration<Data.Entities.Account>
{
    public void Configure(EntityTypeBuilder<Data.Entities.Account> entity)
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
              .HasForeignKey<Data.Entities.Account>(e => e.AccountId);

        entity.HasMany(e => e.VirtualAccounts)
              .WithOne(e => e.Account)
              .HasForeignKey(e => e.AccountId);

    }
}
