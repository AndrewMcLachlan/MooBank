namespace Asm.MooBank.Modules.Bills.Models;

/// <summary>
/// A bill period named by its position rather than its dates.
/// </summary>
/// <remarks>
/// Bills are issued for periods, and which dates those cover is something only the bills know. A
/// caller asking for the last one should not have to fetch bills to work out what to ask for.
/// </remarks>
public enum BillPeriod
{
    /// <summary>The most recently issued bill.</summary>
    Last = 1,

    /// <summary>The one issued before that.</summary>
    Previous = 2,
}
