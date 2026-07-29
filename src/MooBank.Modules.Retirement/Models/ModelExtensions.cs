using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Models;

public static class ModelExtensions
{
    public static RetirementPlan ToModel(this DomainEntities.RetirementPlan plan) =>
        new()
        {
            Id = plan.Id,
            Name = plan.Name,
            ExpectedReturnRate = plan.ExpectedReturnRate,
            InflationRate = plan.InflationRate,
            SuperGuaranteeRate = plan.SuperGuaranteeRate,
            ContributionsTaxRate = plan.ContributionsTaxRate,
            LifeExpectancy = plan.LifeExpectancy,
            CreatedUtc = plan.CreatedUtc,
            UpdatedUtc = plan.UpdatedUtc,
            Members = plan.Members.Select(m => m.ToModel()).ToList(),
        };

    public static IEnumerable<RetirementPlan> ToModel(this IEnumerable<DomainEntities.RetirementPlan> plans) =>
        plans.Select(p => p.ToModel());

    public static RetirementPlanMember ToModel(this DomainEntities.RetirementPlanMember member) =>
        new()
        {
            Id = member.Id,
            Name = member.Name,
            CurrentAge = member.CurrentAge,
            CurrentIncome = member.CurrentIncome,
            SalarySacrifice = member.SalarySacrifice,
            AnnualFees = member.AnnualFees,
            InsurancePremium = member.InsurancePremium,
            RetirementAge = member.RetirementAge,
            GrowthStrategy = member.GrowthStrategy,
            InstrumentIds = member.Accounts.Select(a => a.InstrumentId).ToList(),
        };

    public static DomainEntities.RetirementAssumptions ToAssumptions(this RetirementPlanBase plan) =>
        new(plan.ExpectedReturnRate, plan.InflationRate, plan.SuperGuaranteeRate, plan.ContributionsTaxRate, plan.LifeExpectancy);
}
