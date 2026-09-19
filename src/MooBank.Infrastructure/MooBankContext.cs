#pragma warning disable CS8618
using System.Reflection;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Domain.Entities.Retirement;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure;

public partial class MooBankContext : DomainDbContext, IReadOnlyDbContext
{
    private static readonly List<Assembly> _assemblies = [];

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

    public DbSet<BudgetLine> BudgetLines { get; set; }

    public DbSet<ExchangeRate> ExchangeRates { get; set; }

    public DbSet<Group> Groups { get; set; }

    public DbSet<ImporterType> ImporterTypes { get; set; }

    public DbSet<InstrumentOwner> InstrumentOwners { get; set; }

    public DbSet<TransactionTagTotal> TransactionTagTotals { get; set; }

    public DbSet<MonthlyTagTotal> MonthlyTagTotals { get; set; }

    public DbSet<CreditDebitTotal> CreditDebitTotals { get; set; }

    public DbSet<CreditDebitAverage> CreditDebitAverages { get; set; }

    public DbSet<TagAverage> TopTagAverages { get; set; }

    public DbSet<MonthlyBalance> MonthlyBalances { get; set; }

    public DbSet<MonthlyCreditDebitTotal> MonthlyCreditDebitTotals { get; set; }

    public DbSet<AccountCreditDebitTotal> AccountCreditDebitTotals { get; set; }

    public DbSet<AccountMonthlyBalance> AccountMonthlyBalances { get; set; }

    public DbSet<AccountMonthlyCreditDebitTotal> AccountMonthlyCreditDebitTotals { get; set; }

    public DbSet<StockPriceHistory> StockPriceHistory { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<CpiChange> CpiChanges { get; set; }

    public DbSet<PensionRate> PensionRates { get; set; }

    public DbSet<GrowthStrategyRate> GrowthStrategyRates { get; set; }

    public DbSet<MinimumDrawdownRate> MinimumDrawdownRates { get; set; }

    public DbSet<ForecastPlan> ForecastPlans { get; set; }

    public DbSet<ForecastPlanAccount> ForecastPlanAccounts { get; set; }

    public DbSet<ForecastPlannedItem> ForecastPlannedItems { get; set; }

    public DbSet<PlannedItemFixedDate> PlannedItemFixedDates { get; set; }

    public DbSet<PlannedItemSchedule> PlannedItemSchedules { get; set; }

    public DbSet<RetirementPlan> RetirementPlans { get; set; }

    public DbSet<RetirementPlanMember> RetirementPlanMembers { get; set; }

    public DbSet<RetirementPlanMemberAccount> RetirementPlanMemberAccounts { get; set; }

    // Importer (and other) assemblies contribute their EF configurations to the shared model via this
    // list, which OnModelCreating applies. Registration is order-sensitive: every assembly must be
    // registered before the context is first used. EF caches the built model, so any RegisterAssembly
    // call after first use is silently ignored.
    public static void RegisterAssembly(Assembly assembly) => _assemblies.Add(assembly);

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

        _assemblies.ForEach(a => modelBuilder.ApplyConfigurationsFromAssembly(a));

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
#pragma warning restore CS8618
