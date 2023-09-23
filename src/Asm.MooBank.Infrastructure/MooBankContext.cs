using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.RecurringTransactions;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using MediatR;

namespace Asm.MooBank.Infrastructure;

public partial class MooBankContext : DomainDbContext, IReadOnlyDbContext
{
    private static readonly List<Assembly> Assemblies = new();

    public MooBankContext(IMediator mediator) : base(mediator)
    {
    }

    public MooBankContext(DbContextOptions<MooBankContext> options, IMediator mediator) : base(options, mediator)
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

    [AllowNull]
    public virtual DbSet<RecurringTransaction> RecurringTransactions { get; set; }

    [AllowNull]
    public virtual DbSet<Transaction> Transactions { get; set; }

    [AllowNull]
    public virtual DbSet<Tag> TransactionTags { get; set; }

    [AllowNull]
    public virtual DbSet<Rule> TransactionTagRules { get; set; }

    [AllowNull]
    public virtual DbSet<VirtualAccount> VirtualAccounts { get; set; }

    [AllowNull]
    public virtual DbSet<ImportAccount> ImportAccounts { get; set; }

    [AllowNull]
    public virtual DbSet<ImporterType> ImporterTypes { get; set; }

    public static void RegisterAssembly(Assembly assembly) => Assemblies.Add(assembly);

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

        Assemblies.ForEach(a => modelBuilder.ApplyConfigurationsFromAssembly(a));


        modelBuilder.Entity<AccountGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        /*modelBuilder.Entity<RecurringTransaction>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.DestinationVirtualAccount)
                .WithMany(p => p.RecurringTransactionDestinationVirtualAccount)
                .HasForeignKey(d => d.DestinationVirtualAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RecurringTransaction_VirtualAccount_Destination");

            /*entity.HasOne(d => d.Schedule)
                .WithMany(p => p.RecurringTransaction)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RecurringTransaction_Schedule");* /

            entity.HasOne(d => d.SourceVirtualAccount)
                .WithMany(p => p.RecurringTransactionSourceVirtualAccount)
                .HasForeignKey(d => d.SourceVirtualAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RecurringTransaction_VirtualAccount_Source");
        });*/

        /*modelBuilder.Entity<Schedule>(entity =>
        {
            entity.Property(e => e.ScheduleId).ValueGeneratedNever();

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });*/


        modelBuilder.Entity<TransactionTagTransactionTag>(entity =>
        {
            entity.HasKey(e => new { e.PrimaryTransactionTagId, e.SecondaryTransactionTagId });

            /*entity.HasOne(d => d.Primary)
                .WithMany(p => p.TransactionTagTransactionTagPrimaryTransactionTag)
                .HasForeignKey(d => d.PrimaryTransactionTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransactionTag_TransactionTag_Primary");

            entity.HasOne(d => d.Secondary)
                .WithMany(p => p.TransactionTagTransactionTagSecondaryTransactionTag)
                .HasForeignKey(d => d.SecondaryTransactionTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransactionTag_TransactionTag_Secondary");*/
        });

        modelBuilder.Entity<TransactionTagRuleTransactionTag>(entity =>
        {
            entity.HasKey(e => new { e.TransactionTagRuleId, e.TransactionTagId });
        });


        modelBuilder.Entity<ImporterType>(entity =>
        {
            entity.HasKey(e => e.ImporterTypeId);
        });

    }
}
