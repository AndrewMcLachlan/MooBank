using System.Globalization;

namespace Asm.MooBank.Modules.Budgets.Services;

/// <summary>
/// A single month's aggregated spend (or income) magnitude for one tag.
/// <paramref name="Amount"/> is a non-negative magnitude; <paramref name="Month"/> is 1-12.
/// </summary>
public sealed record MonthlySpend(int Year, int Month, decimal Amount);

/// <summary>
/// A suggested budget line derived from history: the per-occurrence
/// <paramref name="Amount"/>, the <paramref name="Months"/> bitmask it applies to
/// (bit <c>(m-1)</c> for calendar month <c>m</c>), and a short human-readable
/// provenance <paramref name="Note"/>.
/// </summary>
public sealed record BudgetSuggestion(decimal Amount, short Months, string Note);

/// <summary>
/// Classifies a tag's monthly spend history into a realistic budget line.
/// Detects genuine periodic payments (yearly e.g. car registration, quarterly
/// e.g. rates, monthly e.g. electricity) and places them in the right month(s)
/// at the right per-occurrence amount, while rolling lumpy/irregular spend
/// (e.g. medical) up into an annualised monthly average.
/// Pure and deterministic so it can be unit-tested in isolation.
/// </summary>
public static class BudgetSuggestionCalculator
{
    /// <summary>All twelve month bits set (Jan..Dec).</summary>
    public const short AllMonths = 4095;

    // A category counts as "monthly" when it has spend in at least this fraction
    // of the months since its first occurrence (and enough occurrences to be sure).
    private const double MonthlyCoverageThreshold = 0.6;
    private const int MinMonthlyOccurrences = 3;

    // A repeating pattern is only "periodic" if both the gaps between payments and
    // the amounts are reasonably consistent; otherwise it's just lumpy spend.
    private const double RegularityThreshold = 0.35;   // max coefficient of variation

    // Inclusive month-gap ranges that map an average interval to a cadence.
    private const int YearlyMinGap = 10, YearlyMaxGap = 15;
    private const int SubYearlyMinGap = 2, SubYearlyMaxGap = 7;   // quarterly .. half-yearly

    /// <summary>
    /// Produces a budget-line suggestion for a tag from its monthly history, or
    /// <see langword="null"/> when there is no positive spend to base one on.
    /// </summary>
    public static BudgetSuggestion? Calculate(IReadOnlyCollection<MonthlySpend> series)
    {
        var spends = series.Where(s => s.Amount > 0m).OrderBy(AbsoluteMonth).ToList();
        if (spends.Count == 0) return null;

        var total = spends.Sum(s => s.Amount);
        if (total <= 0m) return null;

        var firstAbs = AbsoluteMonth(spends[0]);
        var lastAbs = AbsoluteMonth(spends[^1]);
        var span = lastAbs - firstAbs + 1;              // months from first to last occurrence
        var occurrences = spends.Count;
        var coverage = (double)occurrences / span;

        // Monthly: present in most months since it first appeared.
        if (occurrences >= MinMonthlyOccurrences && coverage >= MonthlyCoverageThreshold)
        {
            return new BudgetSuggestion(Round(total / occurrences), AllMonths, "Auto: monthly average");
        }

        // Periodic: ≥2 payments spaced at a regular interval, with stable amounts.
        // Cadence is inferred from the gap between payments (≈12 months → yearly,
        // ≈3 → quarterly), so two annual payments are recognised even when they fall
        // in the same calendar year or drift between adjacent months.
        if (occurrences >= 2)
        {
            var gaps = new List<double>();
            for (var i = 1; i < spends.Count; i++) gaps.Add(AbsoluteMonth(spends[i]) - AbsoluteMonth(spends[i - 1]));
            var meanGap = gaps.Average();

            var amountsStable = CoefficientOfVariation(spends.Select(s => (double)s.Amount)) <= RegularityThreshold;
            var gapsRegular = CoefficientOfVariation(gaps) <= RegularityThreshold;

            if (amountsStable && gapsRegular)
            {
                if (meanGap >= YearlyMinGap && meanGap <= YearlyMaxGap)
                {
                    var month = spends[^1].Month;       // most recent occurrence best predicts the next
                    var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                    return new BudgetSuggestion(Round(total / occurrences), ToMask([month]), $"Auto: yearly · {monthName}");
                }

                if (meanGap >= SubYearlyMinGap && meanGap <= SubYearlyMaxGap)
                {
                    var months = spends.Select(s => s.Month).Distinct().ToList();
                    var label = months.Count == 4 ? "quarterly" : "periodic";
                    return new BudgetSuggestion(Round(total / occurrences), ToMask(months), $"Auto: {label}");
                }
            }
        }

        // Irregular / lumpy: roll up to an annualised monthly average.
        var yearsOfData = Math.Max(1, (int)Math.Round(span / 12.0, MidpointRounding.AwayFromZero));
        return new BudgetSuggestion(Round(total / yearsOfData / 12m), AllMonths, "Auto: averaged");
    }

    private static double CoefficientOfVariation(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (list.Count <= 1) return 0;
        var mean = list.Average();
        if (mean == 0) return 0;
        var variance = list.Sum(v => (v - mean) * (v - mean)) / list.Count;
        return Math.Sqrt(variance) / Math.Abs(mean);
    }

    private static int AbsoluteMonth(MonthlySpend s) => (s.Year * 12) + (s.Month - 1);

    private static short ToMask(IEnumerable<int> months)
    {
        var mask = 0;
        foreach (var m in months) mask |= 1 << (m - 1);
        return (short)mask;
    }

    // Round to a step that scales with magnitude, so suggestions read like a budget
    // ($90, $210, $1,500) rather than transaction-precise figures ($87.53, $1,487.21).
    private static decimal Round(decimal value)
    {
        var step = Math.Abs(value) switch
        {
            < 50m => 1m,
            < 200m => 5m,
            < 1000m => 10m,
            < 5000m => 50m,
            _ => 100m,
        };
        return Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
    }
}
