namespace Asm.MooBank;

/// <summary>
/// What a metered quantity on a bill measures.
/// </summary>
public enum UsageType
{
    Consumption = 1,
    /// <summary>
    /// Electricity sent back to the grid, credited at a feed-in tariff rather than charged.
    /// </summary>
    Export = 2,
}
