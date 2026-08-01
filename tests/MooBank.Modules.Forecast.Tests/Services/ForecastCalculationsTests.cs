#nullable enable
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Services;

namespace Asm.MooBank.Modules.Forecast.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ForecastCalculations"/> — the pure numeric core of the forecast engine
/// (regression fitting, baseline recalculation from actuals, and summary generation), focusing on the
/// edge cases where wrong maths silently produces wrong forecasts.
/// </summary>
[Trait("Category", "Unit")]
public class ForecastCalculationsTests
{
    private static readonly IncomeCorrelatedSettings Settings = new() { MinDataPoints = 6, RSquaredThreshold = 0.5m };

    #region FitRegression

    /// <summary>
    /// Given fewer data points than the configured minimum
    /// When the regression is fitted
    /// Then the model is invalid but still reports the average income.
    /// </summary>
    [Fact]
    public void FitRegression_FewerThanMinDataPoints_ReturnsInvalid()
    {
        var data = new Dictionary<DateOnly, (decimal Income, decimal Expense)>
        {
            [new(2024, 1, 1)] = (1000m, 500m),
            [new(2024, 2, 1)] = (2000m, 700m),
        };

        var model = ForecastCalculations.FitRegression(data, Settings);

        Assert.False(model.Valid);
        Assert.Equal(1500m, model.AvgHistoricalIncome);
    }

    /// <summary>
    /// Given enough data points but zero variance in income
    /// When the regression is fitted
    /// Then it cannot be fitted and the model is invalid (guards divide-by-zero).
    /// </summary>
    [Fact]
    public void FitRegression_ZeroIncomeVariance_ReturnsInvalid()
    {
        var data = new Dictionary<DateOnly, (decimal Income, decimal Expense)>();
        for (var i = 0; i < 6; i++)
        {
            data[new DateOnly(2024, 1, 1).AddMonths(i)] = (1000m, 400m + i);
        }

        var model = ForecastCalculations.FitRegression(data, Settings);

        Assert.False(model.Valid);
        Assert.Equal(1000m, model.AvgHistoricalIncome);
    }

    /// <summary>
    /// Given points lying exactly on expense = 100 + 0.5 * income
    /// When the regression is fitted
    /// Then it recovers the slope and intercept with R-squared 1 and is valid.
    /// </summary>
    [Fact]
    public void FitRegression_PerfectLinearFit_ReturnsValidModel()
    {
        var data = new Dictionary<DateOnly, (decimal Income, decimal Expense)>();
        for (var i = 0; i < 6; i++)
        {
            var income = 1000m * (i + 1);
            data[new DateOnly(2024, 1, 1).AddMonths(i)] = (income, 100m + 0.5m * income);
        }

        var model = ForecastCalculations.FitRegression(data, Settings);

        Assert.True(model.Valid);
        Assert.Equal(0.5m, model.Slope, 4);
        Assert.Equal(100m, model.Intercept, 4);
        Assert.Equal(1m, model.RSquared, 4);
    }

    /// <summary>
    /// Given a strong fit but a negative slope (expense falls as income rises)
    /// When the regression is fitted
    /// Then the model is rejected as invalid (nonsensical for expense forecasting).
    /// </summary>
    [Fact]
    public void FitRegression_NegativeSlope_ReturnsInvalid()
    {
        var data = new Dictionary<DateOnly, (decimal Income, decimal Expense)>();
        for (var i = 0; i < 6; i++)
        {
            var income = 1000m * (i + 1);
            data[new DateOnly(2024, 1, 1).AddMonths(i)] = (income, 5000m - 0.5m * income);
        }

        var model = ForecastCalculations.FitRegression(data, Settings);

        Assert.False(model.Valid);
    }

    /// <summary>
    /// Given no data points
    /// When the regression is fitted
    /// Then it is invalid with zero average income (no divide-by-zero on the average).
    /// </summary>
    [Fact]
    public void FitRegression_NoData_ReturnsZeroAverageIncome()
    {
        var model = ForecastCalculations.FitRegression([], Settings);

        Assert.False(model.Valid);
        Assert.Equal(0m, model.AvgHistoricalIncome);
    }

    #endregion

    #region RecalculateBaselineFromActuals

    /// <summary>
    /// Given no actual balance data
    /// When the baseline is recalculated
    /// Then the fallback baseline is returned.
    /// </summary>
    [Fact]
    public void RecalculateBaseline_NoActuals_ReturnsFallback()
    {
        var result = ForecastCalculations.RecalculateBaselineFromActuals(
            [], [], [],
            new DateOnly(2024, 1, 1), new DateOnly(2024, 2, 1),
            fallbackBaseline: 1234m);

        Assert.Equal(1234m, result);
    }

    /// <summary>
    /// Given consecutive actual balances for two months
    /// When the baseline is recalculated
    /// Then it returns the average of the derived outgoings (opening + income + planned - closing).
    /// </summary>
    [Fact]
    public void RecalculateBaseline_ConsecutiveActuals_AveragesDerivedOutgoings()
    {
        var balances = new Dictionary<string, decimal?>
        {
            ["2024-01"] = 1000m,
            ["2024-02"] = 900m,
            ["2024-03"] = 750m,
        };
        var income = new Dictionary<string, decimal> { ["2024-01"] = 500m, ["2024-02"] = 500m };

        var result = ForecastCalculations.RecalculateBaselineFromActuals(
            balances, income, [],
            new DateOnly(2024, 1, 1), new DateOnly(2024, 2, 1),
            fallbackBaseline: 0m);

        // Jan: 1000 + 500 - 900 = 600; Feb: 900 + 500 - 750 = 650; average = 625.
        Assert.Equal(625m, result);
    }

    /// <summary>
    /// Given a month containing a planned expense that was actually paid
    /// When the baseline is recalculated
    /// Then the planned expense is not counted as baseline spending
    /// </summary>
    /// <remarks>
    /// The balance change holds everything that was spent, planned or not. Planned expenses are
    /// added to the forecast on their own, so leaving them in the derived baseline would charge the
    /// forecast for them twice — and, because the baseline is an average applied to every future
    /// month, a single large one-off would be spread across the whole plan as if it recurred.
    /// </remarks>
    [Fact]
    public void RecalculateBaseline_PlannedExpense_IsNotCountedAsBaseline()
    {
        var balances = new Dictionary<string, decimal?>
        {
            ["2024-01"] = 10000m,
            ["2024-02"] = 4000m, // 5000 of ordinary spending, plus a 2000 planned expense
        };
        var credits = new Dictionary<string, decimal> { ["2024-01"] = 1000m };
        var plannedExpenses = new Dictionary<string, decimal> { ["2024-01"] = 2000m };

        var result = ForecastCalculations.RecalculateBaselineFromActuals(
            balances, credits, plannedExpenses,
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1),
            fallbackBaseline: 0m);

        // Spent = 10000 + 1000 - 4000 = 7000, of which 2000 was planned.
        Assert.Equal(5000m, result);
    }

    /// <summary>
    /// Given a month whose derived outgoings are negative (unexplained balance growth)
    /// When the baseline is recalculated
    /// Then that month is skipped; with no usable months the fallback is returned.
    /// </summary>
    [Fact]
    public void RecalculateBaseline_NegativeDerived_SkipsMonth()
    {
        var balances = new Dictionary<string, decimal?>
        {
            ["2024-01"] = 1000m,
            ["2024-02"] = 2000m, // balance grew with no income → derived outgoings negative
        };

        var result = ForecastCalculations.RecalculateBaselineFromActuals(
            balances, [], [],
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1),
            fallbackBaseline: 42m);

        Assert.Equal(42m, result);
    }

    #endregion

    #region CalculateSummary

    /// <summary>
    /// Given no forecast months
    /// When the summary is calculated
    /// Then a zeroed summary is returned.
    /// </summary>
    [Fact]
    public void CalculateSummary_NoMonths_ReturnsZeroedSummary()
    {
        var summary = ForecastCalculations.CalculateSummary([], monthlyBaselineOutgoings: 500m);

        Assert.Equal(0m, summary.LowestBalance);
        Assert.Equal(0, summary.MonthsBelowZero);
        Assert.Equal(0m, summary.TotalOutgoings);
    }

    /// <summary>
    /// Given months where the balance dips below zero
    /// When the summary is calculated
    /// Then the lowest balance, months-below-zero and required monthly uplift are computed.
    /// </summary>
    [Fact]
    public void CalculateSummary_BalanceBelowZero_ComputesUpliftAndCount()
    {
        var months = new List<ForecastMonth>
        {
            new() { MonthStart = new(2024, 1, 1), ClosingBalance = 100m, IncomeTotal = 1000m, BaselineOutgoingsTotal = -900m },
            new() { MonthStart = new(2024, 2, 1), ClosingBalance = -200m, IncomeTotal = 1000m, BaselineOutgoingsTotal = -1200m },
        };

        var summary = ForecastCalculations.CalculateSummary(months, monthlyBaselineOutgoings: 1000m);

        Assert.Equal(-200m, summary.LowestBalance);
        Assert.Equal(new DateOnly(2024, 2, 1), summary.LowestBalanceMonth);
        Assert.Equal(1, summary.MonthsBelowZero);
        // lowest is month 2 (index 1) → monthsUntilLow = 2 → uplift = 200 / 2 = 100.
        Assert.Equal(100m, summary.RequiredMonthlyUplift);
    }

    /// <summary>
    /// Given a valid regression model
    /// When the summary is calculated
    /// Then the effective baseline comes from the months' averaged outgoings and the regression
    /// diagnostics report that it did not fall back to the flat average.
    /// </summary>
    [Fact]
    public void CalculateSummary_ValidRegression_UsesRegressionDiagnostics()
    {
        var months = new List<ForecastMonth>
        {
            new() { MonthStart = new(2024, 1, 1), ClosingBalance = 500m, BaselineOutgoingsTotal = -800m },
            new() { MonthStart = new(2024, 2, 1), ClosingBalance = 400m, BaselineOutgoingsTotal = -1200m },
        };
        var regression = new RegressionModel(Intercept: 100m, Slope: 0.5m, RSquared: 0.9m, Valid: true, AvgHistoricalIncome: 3000m);

        var summary = ForecastCalculations.CalculateSummary(months, monthlyBaselineOutgoings: 999m, regression);

        Assert.Equal(1000m, summary.MonthlyBaselineOutgoings); // average of |−800|, |−1200|
        Assert.NotNull(summary.Regression);
        Assert.False(summary.Regression!.FellBackToFlatAverage);
        Assert.Equal(0.5m, summary.Regression.VariableComponent);
    }

    #endregion
}
