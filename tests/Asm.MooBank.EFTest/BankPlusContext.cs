using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Infrastructure;

public partial class BankPlusContext : DbContext
{
    public BankPlusContext()
    {
    }

    public BankPlusContext(DbContextOptions<BankPlusContext> options) : base(options)
    {
    }

    [AllowNull]
    public virtual DbSet<Account> Accounts { get; set; }

    [AllowNull]
    public virtual DbSet<AccountAccountHolder> AccountAccountHolder { get; set; }

    [AllowNull]
    public virtual DbSet<AccountHolder> AccountHolders { get; set; }

    [AllowNull]
    public virtual DbSet<AccountGroup> AccountGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.ClrType.Name);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);


        modelBuilder.Entity<AccountGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<AccountHolder>(entity =>
        {
            entity.HasIndex(e => e.EmailAddress)
                .HasDatabaseName("IX_AccountHolder_Email")
                .IsUnique();

            entity.HasKey(e => e.AccountHolderId);

            entity.Property(e => e.EmailAddress)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FirstName)
                .HasMaxLength(255);

            entity.Property(e => e.LastName)
                .HasMaxLength(255);
        });



    }
}
