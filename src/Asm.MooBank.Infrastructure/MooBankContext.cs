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

        modelBuilder.Entity<TransactionTagTransactionTag>(entity =>
        {
            entity.HasKey(e => new { e.PrimaryTransactionTagId, e.SecondaryTransactionTagId });
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
