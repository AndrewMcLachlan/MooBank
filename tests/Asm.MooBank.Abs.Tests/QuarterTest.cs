using Asm.MooBank.Models;

namespace Asm.MooBank.Abs.Tests;

public class QuarterTest
{
    [Theory]
    [InlineData("2025-05-31", 6, "2024-Q4")]
    [InlineData("2025-05-31", 1, "2025-Q2")]
    public void ParseQuarter(string quarter, int startMonth, string expected)
    {
        Quarter quarter1 = Quarter.FromDate(DateTime.Parse(quarter), startMonth);

        Assert.Equal(expected, quarter1.ToString());
    }
}
