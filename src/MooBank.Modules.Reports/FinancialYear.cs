namespace Asm.MooBank.Modules.Reports;

/// <summary>
/// Australian Financial Year (1 July – 30 June) helpers.
/// </summary>
public static class FinancialYear
{
    /// <summary>
    /// Returns the AU financial year window that contains <paramref name="date"/>.
    /// </summary>
    /// <remarks>
    /// The FY end year is the calendar year of 30 June at the close of the window
    /// (e.g. the FY containing 5 Aug 2025 is 1 Jul 2025 – 30 Jun 2026, labelled FY2026).
    /// </remarks>
    public static (DateOnly Start, DateOnly End, int Year) For(DateOnly date)
    {
        if (date.Month >= 7)
        {
            return (new DateOnly(date.Year, 7, 1), new DateOnly(date.Year + 1, 6, 30), date.Year + 1);
        }
        return (new DateOnly(date.Year - 1, 7, 1), new DateOnly(date.Year, 6, 30), date.Year);
    }

    /// <summary>
    /// Enumerates each AU financial year window between two dates inclusive,
    /// in chronological order. Both dates are mapped to their containing FY first.
    /// </summary>
    public static IEnumerable<(DateOnly Start, DateOnly End, int Year)> Range(DateOnly start, DateOnly end)
    {
        var first = For(start);
        var last = For(end);

        for (var year = first.Year; year <= last.Year; year++)
        {
            yield return (new DateOnly(year - 1, 7, 1), new DateOnly(year, 6, 30), year);
        }
    }
}
