using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure;

public partial class MooBankContext : DomainDbContext, IReadOnlyDbContext
{
    private static readonly List<Assembly> Assemblies = [];

    private readonly Security.IUserDataProvider? _userDataProvider;

    public MooBankContext(IPublisher publisher) : base(publisher)
    {
    }

    public MooBankContext(DbContextOptions<MooBankContext> options, IPublisher publisher) : base(options, publisher)
    {
    }

    public MooBankContext(DbContextOptions<MooBankContext> options, IPublisher publisher, IUserDataProvider userDataProvider) : base(options, publisher)
    {
        _userDataProvider = userDataProvider;
    }

    /// <summary>
    /// The current user's family, used by the family query filter. Evaluated per query, so a user
    /// set after construction (e.g. background processing via <c>ISettableUserDataProvider</c>) is honoured.
    /// Resolves to <see cref="Guid.Empty"/> when there is no current user, so family-filtered queries
    /// are fail-closed; system paths that legitimately span tenants must use <c>IgnoreQueryFilters</c>.
    /// </summary>
    private Guid CurrentFamilyId
    {
        get
        {
            try
            {
                return _userDataProvider?.GetCurrentUser()?.FamilyId ?? Guid.Empty;
            }
            catch (InvalidOperationException)
            {
                return Guid.Empty;
            }
        }
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
    public virtual DbSet<MonthlyCreditDebitTotal> MonthlyCreditDebitTotals { get; set; }

    [AllowNull]
    public virtual DbSet<AccountCreditDebitTotal> AccountCreditDebitTotals { get; set; }

    [AllowNull]
    public virtual DbSet<AccountMonthlyBalance> AccountMonthlyBalances { get; set; }

    [AllowNull]
    public virtual DbSet<AccountMonthlyCreditDebitTotal> AccountMonthlyCreditDebitTotals { get; set; }

    [AllowNull]
    public virtual DbSet<StockPriceHistory> StockPriceHistory { get; set; }

    [AllowNull]
    public virtual DbSet<User> Users { get; set; }

    [AllowNull]
    public virtual DbSet<CpiChange> CpiChanges { get; set; }

    [AllowNull]
    public virtual DbSet<ForecastPlan> ForecastPlans { get; set; }

    [AllowNull]
    public virtual DbSet<ForecastPlanAccount> ForecastPlanAccounts { get; set; }

    [AllowNull]
    public virtual DbSet<ForecastPlannedItem> ForecastPlannedItems { get; set; }

    [AllowNull]
    public virtual DbSet<PlannedItemFixedDate> PlannedItemFixedDates { get; set; }

    [AllowNull]
    public virtual DbSet<PlannedItemSchedule> PlannedItemSchedules { get; set; }

    [AllowNull]
    public virtual DbSet<PlannedItemFlexibleWindow> PlannedItemFlexibleWindows { get; set; }

    // Importer (and other) assemblies contribute their EF configurations to the shared model via this
    // list, which OnModelCreating applies. Registration is order-sensitive: every assembly must be
    // registered before the context is first used. EF caches the built model, so any RegisterAssembly
    // call after first use is silently ignored.
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

        // Named query filters: "Family" applies unconditionally (never ignored outside system paths);
        // "SoftDelete" may be selectively lifted per query via IgnoreQueryFilters(["SoftDelete"])
        // (e.g. historical transaction views, trend reports on deleted tags).
        modelBuilder.Entity<Domain.Entities.Tag.Tag>()
            .HasQueryFilter("Family", t => t.FamilyId == CurrentFamilyId)
            .HasQueryFilter("SoftDelete", t => !t.Deleted);

        modelBuilder.Entity<TransactionTagTotal>().HasNoKey();
        modelBuilder.Entity<MonthlyTagTotal>().HasNoKey();
        modelBuilder.Entity<CreditDebitTotal>().HasNoKey();
        modelBuilder.Entity<CreditDebitAverage>().HasNoKey();
        modelBuilder.Entity<TagAverage>().HasNoKey();
        modelBuilder.Entity<MonthlyBalance>().HasNoKey();
        modelBuilder.Entity<MonthlyCreditDebitTotal>().HasNoKey();
        modelBuilder.Entity<AccountCreditDebitTotal>().HasNoKey();
        modelBuilder.Entity<AccountMonthlyBalance>().HasNoKey();
        modelBuilder.Entity<AccountMonthlyCreditDebitTotal>().HasNoKey();

        modelBuilder.HasDbFunction(typeof(Transaction).GetMethod(nameof(Transaction.TransactionNetAmount), [typeof(TransactionType), typeof(Guid), typeof(decimal)])!);
        modelBuilder.HasDbFunction(typeof(TransactionSplit).GetMethod(nameof(TransactionSplit.TransactionSplitNetAmount), [typeof(Guid), typeof(Guid), typeof(decimal)])!);
    }
}
