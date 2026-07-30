#nullable enable
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using DomainPlan = Asm.MooBank.Domain.Entities.Retirement.RetirementPlan;

namespace Asm.MooBank.Modules.Retirement.Tests.Support;

internal static class EngineExtensions
{
    /// <summary>
    /// Runs a projection with no Age Pension at all.
    /// </summary>
    /// <remarks>
    /// Most tests are about superannuation arithmetic, where a pension would add income they are not
    /// asserting on and, worse, would slow the drawdown they are measuring. Named rather than made a
    /// default on the engine itself, so a caller in the application can never leave the pension out
    /// by accident — the tests that do want it pass rates explicitly.
    /// </remarks>
    public static RetirementProjection CalculateWithoutPension(
        this RetirementProjectionEngine engine,
        DomainPlan plan,
        DateOnly today,
        ProjectionOverrides? overrides = null) =>
        engine.Calculate(plan, today, AgePensionRates.None, overrides);
}
