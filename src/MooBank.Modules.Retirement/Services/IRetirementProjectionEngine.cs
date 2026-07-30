using Asm.MooBank.Modules.Retirement.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Services;

public interface IRetirementProjectionEngine
{
    /// <summary>
    /// Project a plan's superannuation balances forward to retirement.
    /// </summary>
    /// <param name="plan">The plan, with its members and their instruments loaded.</param>
    /// <param name="today">The date the projection starts from.</param>
    /// <param name="overrides">
    /// Values to run under instead of the plan's own. Used by the tweak sliders; the plan is never
    /// modified.
    /// </param>
    RetirementProjection Calculate(DomainEntities.RetirementPlan plan, DateOnly today, AgePensionRates pensionRates, ProjectionOverrides? overrides = null);
}
