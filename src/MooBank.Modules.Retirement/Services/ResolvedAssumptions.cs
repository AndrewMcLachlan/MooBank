using Asm.MooBank.Modules.Retirement.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// The values a projection actually runs under: what the plan holds, with any overrides applied on
/// top.
/// </summary>
/// <remarks>
/// Resolving into a separate value keeps the tweak sliders from ever touching the tracked entity.
/// A projection run with overrides must not be able to save them by accident.
/// </remarks>
internal sealed record ResolvedAssumptions
{
    public required decimal ExpectedReturnRate { get; init; }

    public required decimal InflationRate { get; init; }

    public required decimal SuperGuaranteeRate { get; init; }

    public required decimal ContributionsTaxRate { get; init; }

    public required int LifeExpectancy { get; init; }

    public static ResolvedAssumptions From(DomainEntities.RetirementPlan plan, ProjectionOverrides? overrides) =>
        new()
        {
            ExpectedReturnRate = overrides?.ExpectedReturnRate ?? plan.ExpectedReturnRate,
            InflationRate = overrides?.InflationRate ?? plan.InflationRate,
            SuperGuaranteeRate = overrides?.SuperGuaranteeRate ?? plan.SuperGuaranteeRate,
            ContributionsTaxRate = overrides?.ContributionsTaxRate ?? plan.ContributionsTaxRate,
            LifeExpectancy = overrides?.LifeExpectancy ?? plan.LifeExpectancy,
        };
}

/// <summary>
/// One member's inputs, with any overrides applied on top.
/// </summary>
internal sealed record ResolvedMember
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required int CurrentAge { get; init; }

    public required decimal CurrentIncome { get; init; }

    public required decimal SalarySacrifice { get; init; }

    public required int RetirementAge { get; init; }

    public required GrowthStrategy GrowthStrategy { get; init; }

    public required decimal Balance { get; init; }

    public static ResolvedMember From(DomainEntities.RetirementPlanMember member, ProjectionOverrides? overrides, decimal balance)
    {
        // An override naming a member who is not on the plan is ignored; see MemberOverride.
        var over = overrides?.Members.FirstOrDefault(m => m.MemberId == member.Id);

        return new ResolvedMember
        {
            Id = member.Id,
            Name = member.Name,
            CurrentAge = over?.CurrentAge ?? member.CurrentAge,
            CurrentIncome = over?.CurrentIncome ?? member.CurrentIncome,
            SalarySacrifice = over?.SalarySacrifice ?? member.SalarySacrifice,
            RetirementAge = over?.RetirementAge ?? member.RetirementAge,
            GrowthStrategy = over?.GrowthStrategy ?? member.GrowthStrategy,
            Balance = balance,
        };
    }
}
