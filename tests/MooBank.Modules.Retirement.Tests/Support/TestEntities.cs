#nullable enable
using Asm.MooBank.Models;
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
        IEnumerable<DomainPlanMember>? members = null) =>
        new(id ?? Guid.NewGuid())
        {
            Name = name ?? Faker.Lorem.Sentence(3),
            FamilyId = familyId ?? Guid.NewGuid(),
            ExpectedReturnRate = expectedReturnRate,
            InflationRate = inflationRate,
            SuperGuaranteeRate = superGuaranteeRate,
            ContributionsTaxRate = contributionsTaxRate,
            LifeExpectancy = lifeExpectancy,
            TargetRetirementIncome = targetRetirementIncome,
            CashBucketYears = cashBucketYears,
            CashReturnRate = cashReturnRate ?? expectedReturnRate,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            Members = members?.ToList() ?? [],
        };

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
