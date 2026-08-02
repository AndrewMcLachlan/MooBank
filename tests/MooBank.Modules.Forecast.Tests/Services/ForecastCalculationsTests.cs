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
    /// Given a strong fit whose slope exceeds one
    /// When the regression is fitted
    /// Then the model is rejected
    /// </summary>
    /// <remarks>
    /// A slope above one has every extra dollar earned spent more than once over. It fits some
    /// stretches of real data — a windfall that gets spent along with some savings — but projected
    /// forward it says a pay rise makes you worse off, which is not a forecast so much as a
    /// countdown. Better a flat average, reported as one.
    /// </remarks>
    [Fact]
    public void FitRegression_SlopeAboveOne_ReturnsInvalid()
    {
        var data = new Dictionary<DateOnly, (decimal Income, decimal Expense)>();
        for (var i = 0; i < 6; i++)
        {
            var income = 1000m * (i + 1);
            data[new DateOnly(2024, 1, 1).AddMonths(i)] = (income, 100m + 1.4m * income);
        }

        var model = ForecastCalculations.FitRegression(data, Settings);

        Assert.False(model.Valid);
        Assert.Equal(1.4m, model.Slope, 4);      // it fitted
        Assert.Equal(1m, model.RSquared, 4);     // and fitted perfectly
    }

    /// <summary>
    /// Given a fit whose slope is exactly one
    /// When the regression is fitted
    /// Then it should be accepted
    /// </summary>
    /// <remarks>
    /// Spending every additional dollar is believable; spending more than all of it is not. The
    /// boundary belongs on the permitted side.
    /// </remarks>
    [Fact]
    public void FitRegression_SlopeOfExactlyOne_IsAccepted()
    {
        var data = new Dictionary<DateOnly, (decimal Income, decimal Expense)>();
        for (var i = 0; i < 6; i++)
        {
            var income = 1000m * (i + 1);
            data[new DateOnly(2024, 1, 1).AddMonths(i)] = (income, 100m + income);
        }

        var model = ForecastCalculations.FitRegression(data, Settings);

        Assert.True(model.Valid);
        Assert.Equal(1m, model.Slope, 4);
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

    #region AverageOrdinarySpending

    /// <summary>
    /// Given no actual data
    /// When ordinary spending is averaged
    /// Then the fallback is returned.
    /// </summary>
    [Fact]
    public void AverageOrdinarySpending_NoActuals_ReturnsFallback()
    {
        var result = ForecastCalculations.AverageOrdinarySpending(
            [], [], new DateOnly(2024, 1, 1), new DateOnly(2024, 3, 1), fallback: 1234m);

        Assert.Equal(1234m, result);
    }

    /// <summary>
    /// Given spending across several months
    /// When it is averaged
    /// Then only the months the data covers are counted
    /// </summary>
    /// <remarks>
    /// A month with no transactions is a gap in the record, not a month where nothing was spent.
    /// Averaging it in as a nought would drag the baseline down by however many months are missing.
    /// </remarks>
    [Fact]
    public void AverageOrdinarySpending_MonthsWithoutData_AreNotCountedAsNought()
    {
        var actual = new Dictionary<string, (decimal Income, decimal Expense)>
        {
            ["2024-01"] = (5000m, 4000m),
            ["2024-03"] = (5000m, 6000m),
        };

        var result = ForecastCalculations.AverageOrdinarySpending(
            actual, [], new DateOnly(2024, 1, 1), new DateOnly(2024, 3, 1), fallback: 0m);

        // February has no data, so the average is over January and March alone.
        Assert.Equal(5000m, result);
    }

    /// <summary>
    /// Given a month whose spending includes a planned item
    /// When ordinary spending is averaged
    /// Then the planned part is not counted as baseline
    /// </summary>
    [Fact]
    public void AverageOrdinarySpending_PlannedSpending_IsNotCountedAsBaseline()
    {
        var actual = new Dictionary<string, (decimal Income, decimal Expense)> { ["2024-01"] = (5000m, 25_000m) };
        var attributed = new Dictionary<string, decimal> { ["2024-01"] = 20_000m };

        var result = ForecastCalculations.AverageOrdinarySpending(
            actual, attributed, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), fallback: 0m);

        Assert.Equal(5000m, result);
    }

    /// <summary>
    /// Given a transfer between two of the plan's own accounts
    /// When ordinary spending is averaged
    /// Then it should not read as spending
    /// </summary>
    /// <remarks>
    /// The defect this pins down. The baseline used to be inferred from the change in balance:
    /// <c>spent = opening + credits - closing</c>. Balances are raw and count every movement, while
    /// the credit totals honour exclusions — so the two disagreed by exactly the excluded amount and
    /// the difference landed in "spending". A $20,000 "Savings bump" moved between two accounts of
    /// the same plan left the total balance untouched, yet read as $20,000 spent, and being averaged
    /// across the plan it lifted every future month.
    ///
    /// Reading the spending figure directly instead means a transfer is only ever spending if the
    /// spending totals say it is.
    /// </remarks>
    [Fact]
    public void AverageOrdinarySpending_TransferBetweenThePlansOwnAccounts_IsNotSpending()
    {
        // The month's genuine spending is 6,000. A 20,000 transfer moved between the plan's own
        // accounts as well, which the spending totals do not count.
        var actual = new Dictionary<string, (decimal Income, decimal Expense)> { ["2025-09"] = (15_721m, 6_000m) };

        var result = ForecastCalculations.AverageOrdinarySpending(
            actual, [], new DateOnly(2025, 9, 1), new DateOnly(2025, 9, 1), fallback: 0m);

        Assert.Equal(6_000m, result);
    }

    /// <summary>
    /// Given a month whose planned spending exceeds the total recorded
    /// When ordinary spending is averaged
    /// Then it is floored at nought rather than going negative
    /// </summary>
    [Fact]
    public void AverageOrdinarySpending_PlannedExceedsRecorded_IsFlooredAtNought()
    {
        var actual = new Dictionary<string, (decimal Income, decimal Expense)> { ["2024-01"] = (5000m, 1_000m) };
        var attributed = new Dictionary<string, decimal> { ["2024-01"] = 9_000m };

        var result = ForecastCalculations.AverageOrdinarySpending(
            actual, attributed, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 1), fallback: 0m);

        Assert.Equal(0m, result);
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
        var summary = ForecastCalculations.CalculateSummary([], flatAverage: 500m, RegressionModel.None, modelledIncomeShortfall: 0m);

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

        var summary = ForecastCalculations.CalculateSummary(months, flatAverage: 1000m, RegressionModel.None, modelledIncomeShortfall: 0m);

        Assert.Equal(-200m, summary.LowestBalance);
        Assert.Equal(new DateOnly(2024, 2, 1), summary.LowestBalanceMonth);
        Assert.Equal(1, summary.MonthsBelowZero);
        // lowest is month 2 (index 1) → monthsUntilLow = 2 → uplift = 200 / 2 = 100.
        Assert.Equal(100m, summary.RequiredMonthlyUplift);
    }

    /// <summary>
    /// Given a fitted expense model
    /// When the summary is calculated
    /// Then it should report the two components rather than one figure
    /// </summary>
    /// <remarks>
    /// The average is still reported, but as a consequence of the model rather than as the model:
    /// a single number cannot answer what happens to spending when income changes, which is the
    /// whole question the forecast exists to answer.
    /// </remarks>
    [Fact]
    public void CalculateSummary_FittedModel_ReportsFixedAndVariableRatherThanOneFigure()
    {
        var months = new List<ForecastMonth>
        {
            new() { MonthStart = new(2024, 1, 1), ClosingBalance = 500m, BaselineOutgoingsTotal = -800m },
            new() { MonthStart = new(2024, 2, 1), ClosingBalance = 400m, BaselineOutgoingsTotal = -1200m },
        };
        var regression = new RegressionModel(Intercept: 100m, Slope: 0.5m, RSquared: 0.9m, Valid: true, AvgHistoricalIncome: 3000m, DataPoints: 12);

        var summary = ForecastCalculations.CalculateSummary(months, flatAverage: 999m, regression, modelledIncomeShortfall: 250m);

        Assert.False(summary.Expenses.UsingFlatAverage);
        Assert.Equal(100m, summary.Expenses.FixedComponent);
        Assert.Equal(0.5m, summary.Expenses.VariableComponent);
        Assert.Equal(12, summary.Expenses.DataPoints);
        Assert.Equal(250m, summary.Expenses.ModelledIncomeShortfall);
        Assert.Equal(1000m, summary.Expenses.AverageMonthly); // average of |−800|, |−1200|
    }

    /// <summary>
    /// Given a fit that was rejected
    /// When the summary is calculated
    /// Then the fallback to a flat average should be reported rather than passed off as the model
    /// </summary>
    [Fact]
    public void CalculateSummary_RejectedFit_ReportsTheFallback()
    {
        var months = new List<ForecastMonth>
        {
            new() { MonthStart = new(2024, 1, 1), ClosingBalance = 500m, BaselineOutgoingsTotal = -999m },
        };
        var rejected = new RegressionModel(Intercept: 100m, Slope: 0.5m, RSquared: 0.1m, Valid: false, AvgHistoricalIncome: 3000m, DataPoints: 7);

        var summary = ForecastCalculations.CalculateSummary(months, flatAverage: 999m, rejected, modelledIncomeShortfall: 0m);

        Assert.True(summary.Expenses.UsingFlatAverage);
        Assert.Equal(999m, summary.Expenses.FlatAverage);

        // No components are offered, because a rejected fit has none worth quoting.
        Assert.Equal(0m, summary.Expenses.FixedComponent);
        Assert.Equal(0m, summary.Expenses.VariableComponent);

        // The R² is still reported, so the reason for the fallback is visible.
        Assert.Equal(0.1m, summary.Expenses.RSquared);
        Assert.Equal(7, summary.Expenses.DataPoints);
    }

    #endregion
}
