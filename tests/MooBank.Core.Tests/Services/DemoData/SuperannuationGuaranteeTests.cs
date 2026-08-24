using Asm.MooBank.Services.DemoData;

namespace Asm.MooBank.Core.Tests.Services.DemoData;

/// <summary>
/// Unit tests for <see cref="SuperannuationGuarantee"/>. The demo super account's contributions are
/// generated at these rates, so a step applied a year early would show in the balance chart.
/// </summary>
public class SuperannuationGuaranteeTests
{
    /// <summary>
    /// Given a date within a financial year
    /// When the rate is looked up
    /// Then the rate legislated for that year is returned.
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(2014, 6, 30, 0.0925)]
    [InlineData(2014, 7, 1, 0.0950)]
    [InlineData(2021, 6, 30, 0.0950)]
    [InlineData(2021, 7, 1, 0.1000)]
    [InlineData(2022, 7, 1, 0.1050)]
    [InlineData(2023, 7, 1, 0.1100)]
    [InlineData(2024, 7, 1, 0.1150)]
    [InlineData(2025, 7, 1, 0.1200)]
    public void RateFor_KnownDate_ReturnsTheLegislatedRate(int year, int month, int day, decimal expected)
    {
        Assert.Equal(expected, SuperannuationGuarantee.RateFor(new DateOnly(year, month, day)));
    }

    /// <summary>
    /// Given a date after the schedule reached its final step
    /// When the rate is looked up
    /// Then the final rate stands rather than the lookup falling through.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RateFor_AfterTheFinalStep_HoldsTheFinalRate()
    {
        Assert.Equal(0.1200m, SuperannuationGuarantee.RateFor(new DateOnly(2030, 1, 1)));
    }

    /// <summary>
    /// Given a date before the first rate in the schedule
    /// When the rate is looked up
    /// Then the earliest rate is used rather than zero, which would silently skip the contribution.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RateFor_BeforeTheSchedule_UsesTheEarliestRate()
    {
        Assert.Equal(0.0925m, SuperannuationGuarantee.RateFor(new DateOnly(2010, 1, 1)));
    }
}
