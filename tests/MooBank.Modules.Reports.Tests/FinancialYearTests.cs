#nullable enable
using Asm.MooBank.Modules.Reports;

namespace Asm.MooBank.Modules.Reports.Tests;

/// <summary>
/// Unit tests for the <see cref="FinancialYear"/> helper.
/// </summary>
[Trait("Category", "Unit")]
public class FinancialYearTests
{
    /// <summary>
    /// Given a date in the first half of the calendar year (Jan–Jun)
    /// When the FY is requested
    /// Then it spans the previous July to that June and the FY number is the date's year.
    /// </summary>
    [Theory]
    [InlineData(2026, 1, 15, 2025, 7, 1, 2026, 6, 30, 2026)]
    [InlineData(2026, 6, 30, 2025, 7, 1, 2026, 6, 30, 2026)]
    [InlineData(2024, 3, 1, 2023, 7, 1, 2024, 6, 30, 2024)]
    public void For_JanToJune_ReturnsContainingFy(int y, int m, int d, int sy, int sm, int sd, int ey, int em, int ed, int fy)
    {
        var result = FinancialYear.For(new DateOnly(y, m, d));
        Assert.Equal(new DateOnly(sy, sm, sd), result.Start);
        Assert.Equal(new DateOnly(ey, em, ed), result.End);
        Assert.Equal(fy, result.Year);
    }

    /// <summary>
    /// Given a date in the second half of the calendar year (Jul–Dec)
    /// When the FY is requested
    /// Then it spans that July to the next June and the FY number is one greater than the date's year.
    /// </summary>
    [Theory]
    [InlineData(2025, 7, 1, 2025, 7, 1, 2026, 6, 30, 2026)]
    [InlineData(2025, 12, 31, 2025, 7, 1, 2026, 6, 30, 2026)]
    [InlineData(2030, 8, 15, 2030, 7, 1, 2031, 6, 30, 2031)]
    public void For_JulToDec_ReturnsContainingFy(int y, int m, int d, int sy, int sm, int sd, int ey, int em, int ed, int fy)
    {
        var result = FinancialYear.For(new DateOnly(y, m, d));
        Assert.Equal(new DateOnly(sy, sm, sd), result.Start);
        Assert.Equal(new DateOnly(ey, em, ed), result.End);
        Assert.Equal(fy, result.Year);
    }

    /// <summary>
    /// Given a range spanning two FYs
    /// When Range is enumerated
    /// Then both FYs are returned in chronological order.
    /// </summary>
    [Fact]
    public void Range_SpansTwoFys()
    {
        var result = FinancialYear.Range(new DateOnly(2025, 1, 15), new DateOnly(2026, 3, 1)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(2025, result[0].Year);
        Assert.Equal(2026, result[1].Year);
    }

    /// <summary>
    /// Given a range entirely within one FY
    /// When Range is enumerated
    /// Then a single FY entry is returned.
    /// </summary>
    [Fact]
    public void Range_WithinSingleFy_ReturnsOne()
    {
        var result = FinancialYear.Range(new DateOnly(2025, 8, 1), new DateOnly(2026, 1, 1)).ToList();
        Assert.Single(result);
        Assert.Equal(2026, result[0].Year);
    }
}
