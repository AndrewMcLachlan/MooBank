using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.User;
using MediatR;

namespace Asm.MooBank.Infrastructure;

public partial class MooBankContext : DomainDbContext, IReadOnlyDbContext
{
    private static readonly List<Assembly> Assemblies = [];

    public MooBankContext(IMediator mediator) : base(mediator)
    {
    }

    public MooBankContext(DbContextOptions<MooBankContext> options, IMediator mediator) : base(options, mediator)
    {
    }

    [AllowNull]
    public virtual DbSet<BudgetLine> BudgetLines { get; set; }

    [AllowNull]
    public virtual DbSet<ExchangeRate> ExchangeRates { get; set; }

    [AllowNull]
    public virtual DbSet<Group> Groups { get; set; }

    [AllowNull]
    public virtual DbSet<ImporterType> ImporterTypes { get; set; }

    [AllowNull]
    public virtual DbSet<InstrumentOwner> InstrumentOwners { get; set; }

    [AllowNull]
    public virtual DbSet<StockPriceHistory> StockPriceHistory{ get; set; }

    [AllowNull]
    public virtual DbSet<User> Users { get; set; }

    [AllowNull]
    public virtual DbSet<VirtualInstrument> VirtualAccounts { get; set; }

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

        modelBuilder.Entity<Asset>().UseTptMappingStrategy();

        modelBuilder.Entity<TransactionInstrument>().ToTable(tb => tb.UseSqlOutputClause(false));

        modelBuilder.Entity<TagRelationship>();

        modelBuilder.HasDbFunction(typeof(Transaction).GetMethod(nameof(Transaction.TransactionNetAmount), [typeof(Guid), typeof(decimal)])!);
        modelBuilder.HasDbFunction(typeof(TransactionSplit).GetMethod(nameof(TransactionSplit.TransactionSplitNetAmount), [typeof(Guid), typeof(Guid), typeof(decimal)])!);
    }
}
