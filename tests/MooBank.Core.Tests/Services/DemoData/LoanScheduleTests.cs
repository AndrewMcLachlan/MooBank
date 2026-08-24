using Asm.MooBank.Services.DemoData;

namespace Asm.MooBank.Core.Tests.Services.DemoData;

/// <summary>
/// Unit tests for <see cref="LoanSchedule"/>, which splits a demo loan repayment into interest and
/// principal. A wrong figure here would be written straight into the demo ledger and read as real.
/// </summary>
public class LoanScheduleTests
{
    /// <summary>
    /// Given a balance owing, a repayment and an annual rate
    /// When the repayment is split
    /// Then interest is one month's interest on the balance and principal is the remainder.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Split_TypicalRepayment_TakesOneMonthOfInterest()
    {
        var (interest, principal) = LoanSchedule.Split(387500m, 2200m, 0.055m);

        // 387,500 * 0.055 / 12 = 1,776.0416...
        Assert.Equal(1776.04m, interest);
        Assert.Equal(423.96m, principal);
    }

    /// <summary>
    /// Given any balance and repayment
    /// When the repayment is split
    /// Then the two portions add back to the repayment exactly.
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(387500, 2200, 0.055)]
    [InlineData(298000.37, 2200, 0.055)]
    [InlineData(35000, 701.35, 0.075)]
    [InlineData(1.11, 701.35, 0.075)]
    public void Split_Always_AddsBackToTheRepayment(decimal balance, decimal repayment, decimal rate)
    {
        var (interest, principal) = LoanSchedule.Split(balance, repayment, rate);

        Assert.Equal(repayment, interest + principal);
    }

    /// <summary>
    /// Given a balance so large that a month's interest exceeds the repayment
    /// When the repayment is split
    /// Then the whole repayment is interest rather than the principal going negative.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Split_InterestExceedsRepayment_ChargesTheWholeRepaymentAsInterest()
    {
        var (interest, principal) = LoanSchedule.Split(1_000_000m, 500m, 0.075m);

        Assert.Equal(500m, interest);
        Assert.Equal(0m, principal);
    }

    /// <summary>
    /// Given a loan that has been overpaid to a negative balance
    /// When a repayment is split
    /// Then no interest is charged rather than a credit being invented.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Split_BalanceAlreadyCleared_ChargesNoInterest()
    {
        var (interest, principal) = LoanSchedule.Split(-250m, 701.35m, 0.075m);

        Assert.Equal(0m, interest);
        Assert.Equal(701.35m, principal);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Split_NegativeRepayment_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LoanSchedule.Split(1000m, -1m, 0.055m));
    }
}
