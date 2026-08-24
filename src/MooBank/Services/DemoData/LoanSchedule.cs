namespace Asm.MooBank.Services.DemoData;

/// <summary>
/// Splits a loan repayment into the interest and principal portions for the period.
/// </summary>
public static class LoanSchedule
{
    /// <summary>
    /// Splits <paramref name="repayment"/> against the balance owing before it is applied.
    /// </summary>
    /// <remarks>
    /// Interest is rounded to the cent and principal takes the remainder, so the two always add
    /// back to the repayment exactly. Anything else leaves the loan ledger a cent adrift from the
    /// payment the bank account shows.
    /// </remarks>
    public static (decimal Interest, decimal Principal) Split(decimal balanceOwing, decimal repayment, decimal annualRate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(repayment);

        var interest = Math.Round(Math.Max(balanceOwing, 0m) * annualRate / 12m, 2, MidpointRounding.AwayFromZero);

        // A repayment that no longer covers the interest is the final one, clearing what is left.
        if (interest > repayment) interest = repayment;

        return (interest, repayment - interest);
    }
}
