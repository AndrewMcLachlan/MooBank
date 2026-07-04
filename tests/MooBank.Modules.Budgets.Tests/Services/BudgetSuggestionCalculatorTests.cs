#nullable enable
using Asm.MooBank.Modules.Budgets.Services;

namespace Asm.MooBank.Modules.Budgets.Tests.Services;

/// <summary>
/// Unit tests for <see cref="BudgetSuggestionCalculator"/>.
/// </summary>
[Trait("Category", "Unit")]
public class BudgetSuggestionCalculatorTests
{
    private const short AllMonths = BudgetSuggestionCalculator.AllMonths;

    private static short MonthBit(params int[] months)
    {
        var mask = 0;
        foreach (var m in months) mask |= 1 << (m - 1);
        return (short)mask;
    }

    /// <summary>
    /// Given a tag with no positive spend
    /// When a suggestion is calculated
    /// Then no suggestion is produced.
    /// </summary>
    [Fact]
    public void Calculate_NoSpend_ReturnsNull()
    {
        var result = BudgetSuggestionCalculator.Calculate([]);

        Assert.Null(result);
    }

    /// <summary>
    /// Given a tag with spend in (nearly) every month across two years
    /// When a suggestion is calculated
    /// Then it is flagged as monthly across all twelve months at the average amount.
    /// </summary>
    [Fact]
    public void Calculate_SpendEveryMonth_IsMonthlyAverage()
    {
        var series = new List<MonthlySpend>();
        foreach (var year in new[] { 2023, 2024 })
            for (var month = 1; month <= 12; month++)
                series.Add(new MonthlySpend(year, month, 200m));

        var result = BudgetSuggestionCalculator.Calculate(series);

        Assert.NotNull(result);
        Assert.Equal(AllMonths, result!.Months);
        Assert.Equal(200m, result.Amount);
        Assert.Contains("monthly", result.Note);
    }

    /// <summary>
    /// Given a payment that recurs in the same single month across two years
    /// When a suggestion is calculated
    /// Then it is flagged as yearly in that month at the full per-year amount.
    /// </summary>
    [Fact]
    public void Calculate_SameMonthEachYear_IsYearly()
    {
        var series = new List<MonthlySpend>
        {
            new(2023, 3, 900m),
            new(2024, 3, 940m),
        };

        var result = BudgetSuggestionCalculator.Calculate(series);

        Assert.NotNull(result);
        Assert.Equal(MonthBit(3), result!.Months);
        Assert.Equal(920m, result.Amount); // mean of the two yearly payments
        Assert.Contains("yearly", result.Note);
    }

    /// <summary>
    /// Given a payment that lands on four evenly spaced months across two years
    /// When a suggestion is calculated
    /// Then it is flagged as quarterly on those months at the per-occurrence amount.
    /// </summary>
    [Fact]
    public void Calculate_FourEvenlySpacedMonths_IsQuarterly()
    {
        var series = new List<MonthlySpend>();
        foreach (var year in new[] { 2023, 2024 })
            foreach (var month in new[] { 1, 4, 7, 10 })
                series.Add(new MonthlySpend(year, month, 400m));

        var result = BudgetSuggestionCalculator.Calculate(series);

        Assert.NotNull(result);
        Assert.Equal(MonthBit(1, 4, 7, 10), result!.Months);
        Assert.Equal(400m, result.Amount);
        Assert.Contains("quarterly", result.Note);
    }

    /// <summary>
    /// Given lumpy, irregular spend scattered across two years
    /// When a suggestion is calculated
    /// Then it is rolled up to an annualised monthly average across all months.
    /// </summary>
    [Fact]
    public void Calculate_IrregularSpend_IsAveragedAcrossAllMonths()
    {
        var series = new List<MonthlySpend>
        {
            new(2023, 2, 600m),
            new(2023, 9, 300m),
            new(2024, 5, 900m),
            new(2024, 11, 600m),
        };
        // Total 2400 over ~2 years -> 100/month.

        var result = BudgetSuggestionCalculator.Calculate(series);

        Assert.NotNull(result);
        Assert.Equal(AllMonths, result!.Months);
        Assert.Equal(100m, result.Amount);
        Assert.Contains("averaged", result.Note);
    }

    /// <summary>
    /// Given a single one-off payment
    /// When a suggestion is calculated
    /// Then it is NOT treated as a recurring monthly amount.
    /// </summary>
    [Fact]
    public void Calculate_SingleOneOff_IsNotMonthly()
    {
        var series = new List<MonthlySpend> { new(2024, 6, 2000m) };

        var result = BudgetSuggestionCalculator.Calculate(series);

        Assert.NotNull(result);
        Assert.Equal(AllMonths, result!.Months);
        Assert.Equal("Auto: averaged", result.Note);
        Assert.True(result.Amount < 2000m, "A one-off should be averaged, not budgeted at full value every month.");
    }

    /// <summary>
    /// Given two annual payments that fall in the same calendar year (e.g. a renewal
    /// date that drifted) roughly twelve months apart
    /// When a suggestion is calculated
    /// Then it is still recognised as a yearly payment, not spread across the year.
    /// </summary>
    [Fact]
    public void Calculate_TwoAnnualPaymentsSpanningSameCalendarYear_IsYearly()
    {
        var series = new List<MonthlySpend>
        {
            new(2025, 2, 2750m),   // ~11 months apart, both in 2025
            new(2025, 12, 3500m),
        };

        var result = BudgetSuggestionCalculator.Calculate(series);

        Assert.NotNull(result);
        Assert.Equal(MonthBit(12), result!.Months); // budgeted in the most recent month
        Assert.Contains("yearly", result.Note);
        Assert.True(result.Amount is >= 3000m and <= 3200m, $"Expected ~mean of the two payments, got {result.Amount}");
    }

    /// <summary>
    /// Given a quarterly pattern seen for only a single year
    /// When a suggestion is calculated
    /// Then the cadence is detected from the gaps between payments (quarterly).
    /// </summary>
    [Fact]
    public void Calculate_QuarterlyWithinOneYear_IsQuarterly()
    {
        var series = new List<MonthlySpend>
        {
            new(2024, 1, 400m),
            new(2024, 4, 400m),
            new(2024, 7, 400m),
            new(2024, 10, 400m),
        };

        var result = BudgetSuggestionCalculator.Calculate(series);

        Assert.NotNull(result);
        Assert.Equal(MonthBit(1, 4, 7, 10), result!.Months);
        Assert.Contains("quarterly", result.Note);
    }
}
