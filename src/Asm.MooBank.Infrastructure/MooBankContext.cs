using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Infrastructure;

public partial class MooBankContext : DomainDbContext, IReadOnlyDbContext
{
    private static readonly List<Assembly> Assemblies = [];

    public MooBankContext(IPublisher publisher) : base(publisher)
    {
    }

    public MooBankContext(DbContextOptions<MooBankContext> options, IPublisher publisher) : base(options, publisher)
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
    public virtual DbSet<TransactionTagTotal> TransactionTagTotals { get; set; }

    [AllowNull]
    public virtual DbSet<MonthlyTagTotal> MonthlyTagTotals { get; set; }

    [AllowNull]
    public virtual DbSet<CreditDebitTotal> CreditDebitTotals { get; set; }

    [AllowNull]
    public virtual DbSet<CreditDebitAverage> CreditDebitAverages { get; set; }

    [AllowNull]
    public virtual DbSet<TagAverage> TopTagAverages { get; set; }

    [AllowNull]
    public virtual DbSet<MonthlyBalance> MonthlyBalances { get; set; }

    [AllowNull]
    public virtual DbSet<StockPriceHistory> StockPriceHistory{ get; set; }

    [AllowNull]
    public virtual DbSet<User> Users { get; set; }

    [AllowNull]
    public virtual DbSet<VirtualInstrument> VirtualAccounts { get; set; }

    [AllowNull]
    public virtual DbSet<CpiChange> CpiChanges { get; set; }

    [AllowNull]
    public virtual DbSet<ForecastPlan> ForecastPlans { get; set; }

    [AllowNull]
    public virtual DbSet<ForecastPlanAccount> ForecastPlanAccounts { get; set; }

    [AllowNull]
    public virtual DbSet<ForecastPlannedItem> ForecastPlannedItems { get; set; }

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

        modelBuilder.Entity<Domain.Entities.Utility.Account>().UseTptMappingStrategy();

        modelBuilder.Entity<TransactionInstrument>().ToTable(tb => tb.UseSqlOutputClause(false));

        modelBuilder.Entity<TagRelationship>();

        modelBuilder.Entity<TransactionTagTotal>().HasNoKey();
        modelBuilder.Entity<MonthlyTagTotal>().HasNoKey();
        modelBuilder.Entity<CreditDebitTotal>().HasNoKey();
        modelBuilder.Entity<CreditDebitAverage>().HasNoKey();
        modelBuilder.Entity<TagAverage>().HasNoKey();
        modelBuilder.Entity<MonthlyBalance>().HasNoKey();

        modelBuilder.HasDbFunction(typeof(Transaction).GetMethod(nameof(Transaction.TransactionNetAmount), [typeof(Models.TransactionType), typeof(Guid), typeof(decimal)])!);
        modelBuilder.HasDbFunction(typeof(TransactionSplit).GetMethod(nameof(TransactionSplit.TransactionSplitNetAmount), [typeof(Guid), typeof(Guid), typeof(decimal)])!);
    }
}
