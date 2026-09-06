#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Bogus;
using DomainAccount = Asm.MooBank.Domain.Entities.Account.LogicalAccount;
using DomainPlan = Asm.MooBank.Domain.Entities.Retirement.RetirementPlan;
using DomainPlanMember = Asm.MooBank.Domain.Entities.Retirement.RetirementPlanMember;
using DomainPlanMemberAccount = Asm.MooBank.Domain.Entities.Retirement.RetirementPlanMemberAccount;

namespace Asm.MooBank.Modules.Retirement.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Rates chosen so the arithmetic in tests stays easy to check by hand: 10% return, no
    /// inflation, 10% employer contributions and no contributions tax.
    /// </summary>
    /// <remarks>
    /// The drawdown phase is off by default: no target income, and a cash rate equal to the expected
    /// return, so the switch to cash cannot change any figure. That keeps a test about accumulation
    /// from also testing those. Tests that want either ask for it.
    /// </remarks>
    public static DomainPlan CreatePlan(
        Guid? id = null,
        string? name = null,
        Guid? familyId = null,
        decimal expectedReturnRate = 0.10m,
        decimal inflationRate = 0m,
        decimal superGuaranteeRate = 0.10m,
        decimal contributionsTaxRate = 0m,
        int lifeExpectancy = 90,
        decimal targetRetirementIncome = 0m,
        int cashBucketYears = 0,
        decimal? cashReturnRate = null,
        IEnumerable<DomainPlanMember>? members = null)
    {
        var list = members?.ToList() ?? [];

        // The rate now belongs to the member, so a plan-level figure is how a test says "everyone
        // earns this". A member given its own rate keeps it.
        foreach (var member in list.Where(m => m.GrowthStrategy == GrowthStrategy.Custom && m.CustomReturnRate is null))
        {
            member.CustomReturnRate = expectedReturnRate;
        }

        return new DomainPlan(id ?? Guid.NewGuid())
        {
            Name = name ?? Faker.Lorem.Sentence(3),
            FamilyId = familyId ?? Guid.NewGuid(),
            InflationRate = inflationRate,
            SuperGuaranteeRate = superGuaranteeRate,
            ContributionsTaxRate = contributionsTaxRate,
            LifeExpectancy = lifeExpectancy,
            TargetRetirementIncome = targetRetirementIncome,
            CashBucketYears = cashBucketYears,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            Members = list,
        };
    }

    /// <summary>
    /// The return assumptions a test runs under.
    /// </summary>
    /// <remarks>
    /// Cash earns what the member earns unless a test says otherwise, so the bucket cannot change a
    /// figure a test about accumulation is asserting on. The named strategies carry the seeded
    /// rates, which is what a test choosing one is asking for.
    /// </remarks>
    public static GrowthStrategyRates StrategyRates(DomainPlan plan, decimal? cashReturnRate = null) =>
        new(new Dictionary<GrowthStrategy, decimal>
        {
            [GrowthStrategy.Conservative] = 0.049m,
            [GrowthStrategy.Balanced] = 0.061m,
            [GrowthStrategy.Growth] = 0.064m,
            [GrowthStrategy.HighGrowth] = 0.068m,
            [GrowthStrategy.Moderate] = 0.057m,
            [GrowthStrategy.Cash] = cashReturnRate ?? plan.Members.Select(m => m.CustomReturnRate).FirstOrDefault() ?? 0m,
        });

    /// <summary>
    /// The legislated minimum drawdown bands, as ATO Schedule 7 sets them.
    /// </summary>
    public static MinimumDrawdownRates MinimumDrawdown() =>
        new(new Dictionary<byte, decimal>
        {
            [0] = 0.04m, [65] = 0.05m, [75] = 0.06m, [80] = 0.07m, [85] = 0.09m, [90] = 0.11m, [95] = 0.14m,
        });

    /// <summary>
    /// A plan member. The <c>User</c> navigation is populated because the projection reads the
    /// member's display name from it.
    /// </summary>
    public static DomainPlanMember CreateMember(
        Guid? id = null,
        string? name = null,
        Guid? userId = null,
        int currentAge = 60,
        decimal currentIncome = 100_000m,
        decimal salarySacrifice = 0m,
        int retirementAge = 65,
        GrowthStrategy growthStrategy = GrowthStrategy.Custom,
        decimal? customReturnRate = null,
        GrowthStrategy? retirementGrowthStrategy = null,
        decimal? retirementCustomReturnRate = null,
        decimal? salaryGrowthRate = null,
        decimal annualFees = 0m,
        decimal insurancePremium = 0m,
        IEnumerable<decimal>? accountBalances = null)
    {
        var memberId = id ?? Guid.NewGuid();
        var personId = userId ?? Guid.NewGuid();

        return new DomainPlanMember(memberId)
        {
            UserId = personId,
            User = CreateDomainUser(personId, name ?? Faker.Name.FirstName()),
            CurrentAge = currentAge,
            CurrentIncome = currentIncome,
            SalarySacrifice = salarySacrifice,
            RetirementAge = retirementAge,
            GrowthStrategy = growthStrategy,
            CustomReturnRate = customReturnRate,
            RetirementGrowthStrategy = retirementGrowthStrategy,
            RetirementCustomReturnRate = retirementCustomReturnRate,
            SalaryGrowthRate = salaryGrowthRate,
            AnnualFees = annualFees,
            InsurancePremium = insurancePremium,
            Accounts = (accountBalances ?? []).Select(balance => CreateMemberAccount(memberId, balance)).ToList(),
        };
    }

    /// <summary>
    /// A user record, used as the member's person. The projection joins first and last name, so a
    /// single name here comes back unchanged.
    /// </summary>
    public static Asm.MooBank.Domain.Entities.User.User CreateDomainUser(Guid id, string firstName) =>
        new(id)
        {
            EmailAddress = $"{firstName.ToLowerInvariant()}@example.com",
            FirstName = firstName,
            FamilyId = Guid.NewGuid(),
        };

    /// <summary>
    /// A member's link to an instrument, with the instrument's balance set directly. The balance
    /// setter on <c>TransactionInstrument</c> exists for exactly this — constructing a known
    /// balance without a database behind it.
    /// </summary>
    public static DomainPlanMemberAccount CreateMemberAccount(Guid memberId, decimal balance)
    {
        var instrumentId = Guid.NewGuid();

        return new DomainPlanMemberAccount(Guid.NewGuid())
        {
            RetirementPlanMemberId = memberId,
            InstrumentId = instrumentId,
            Instrument = CreateSuperAccount(instrumentId, balance),
        };
    }

    public static DomainAccount CreateSuperAccount(Guid id, decimal balance) =>
        new(id, [])
        {
            Name = "Super",
            Currency = "AUD",
            Controller = Controller.Manual,
            AccountType = AccountType.Superannuation,
            Balance = balance,
        };

    public static User CreateUser(Guid? id = null, Guid? familyId = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            EmailAddress = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Currency = "AUD",
            FamilyId = familyId ?? Guid.NewGuid(),
            Accounts = [],
            SharedAccounts = [],
            Groups = [],
        };
}
