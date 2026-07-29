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
    public static DomainPlan CreatePlan(
        Guid? id = null,
        string? name = null,
        Guid? familyId = null,
        decimal expectedReturnRate = 0.10m,
        decimal inflationRate = 0m,
        decimal superGuaranteeRate = 0.10m,
        decimal contributionsTaxRate = 0m,
        int lifeExpectancy = 90,
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
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            Members = members?.ToList() ?? [],
        };

    public static DomainPlanMember CreateMember(
        Guid? id = null,
        string? name = null,
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

        return new DomainPlanMember(memberId)
        {
            Name = name ?? Faker.Name.FirstName(),
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
