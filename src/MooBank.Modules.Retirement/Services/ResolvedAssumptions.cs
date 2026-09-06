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
    public required decimal InflationRate { get; init; }

    public required decimal SuperGuaranteeRate { get; init; }

    public required decimal ContributionsTaxRate { get; init; }

    public required int LifeExpectancy { get; init; }

    public required decimal TargetRetirementIncome { get; init; }

    public required int CashBucketYears { get; init; }

    public static ResolvedAssumptions From(DomainEntities.RetirementPlan plan, ProjectionOverrides? overrides) =>
        new()
        {
            InflationRate = overrides?.InflationRate ?? plan.InflationRate,
            SuperGuaranteeRate = overrides?.SuperGuaranteeRate ?? plan.SuperGuaranteeRate,
            ContributionsTaxRate = overrides?.ContributionsTaxRate ?? plan.ContributionsTaxRate,
            LifeExpectancy = overrides?.LifeExpectancy ?? plan.LifeExpectancy,
            TargetRetirementIncome = overrides?.TargetRetirementIncome ?? plan.TargetRetirementIncome,
            CashBucketYears = overrides?.CashBucketYears ?? plan.CashBucketYears,
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

    public required decimal AnnualFees { get; init; }

    public required decimal InsurancePremium { get; init; }

    public required int RetirementAge { get; init; }

    public required GrowthStrategy GrowthStrategy { get; init; }

    /// <summary>
    /// The rate behind <see cref="GrowthStrategy.Custom"/>, and nothing at all otherwise.
    /// </summary>
    public required decimal? CustomReturnRate { get; init; }

    /// <summary>
    /// What this member moves to once retired, or nothing to stay where they are.
    /// </summary>
    public required GrowthStrategy? RetirementGrowthStrategy { get; init; }

    /// <inheritdoc cref="CustomReturnRate"/>
    public required decimal? RetirementCustomReturnRate { get; init; }

    /// <summary>
    /// How their pay grows, or nothing to follow the plan's inflation.
    /// </summary>
    public required decimal? SalaryGrowthRate { get; init; }

    public required decimal Balance { get; init; }

    public static ResolvedMember From(DomainEntities.RetirementPlanMember member, ProjectionOverrides? overrides, decimal balance)
    {
        // An override naming a member who is not on the plan is ignored; see MemberOverride.
        var over = overrides?.Members.FirstOrDefault(m => m.MemberId == member.Id);

        return new ResolvedMember
        {
            Id = member.Id,
            Name = member.User is null ? "" : $"{member.User.FirstName} {member.User.LastName}".Trim(),
            CurrentAge = over?.CurrentAge ?? member.CurrentAge,
            CurrentIncome = over?.CurrentIncome ?? member.CurrentIncome,
            SalarySacrifice = over?.SalarySacrifice ?? member.SalarySacrifice,
            AnnualFees = over?.AnnualFees ?? member.AnnualFees,
            InsurancePremium = over?.InsurancePremium ?? member.InsurancePremium,
            RetirementAge = over?.RetirementAge ?? member.RetirementAge,
            GrowthStrategy = over?.GrowthStrategy ?? member.GrowthStrategy,
            // Moving the rate is what puts a member on Custom, so the two arrive together and
            // an override of one must not be read beside the other's saved value.
            CustomReturnRate = over is not null && (over.GrowthStrategy is not null || over.CustomReturnRate is not null)
                ? over.CustomReturnRate
                : member.CustomReturnRate,
            RetirementGrowthStrategy = over?.RetirementGrowthStrategy ?? member.RetirementGrowthStrategy,
            RetirementCustomReturnRate = over is not null && (over.RetirementGrowthStrategy is not null || over.RetirementCustomReturnRate is not null)
                ? over.RetirementCustomReturnRate
                : member.RetirementCustomReturnRate,
            SalaryGrowthRate = over?.SalaryGrowthRate ?? member.SalaryGrowthRate,
            Balance = balance,
        };
    }
}
