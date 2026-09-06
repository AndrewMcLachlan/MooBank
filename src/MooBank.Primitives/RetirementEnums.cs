namespace Asm.MooBank;

/// <summary>
/// The investment option a superannuation balance is invested in. Each named strategy takes its
/// assumed long-run return from reference data, so a change applies to every plan at once;
/// <see cref="Custom"/> instead carries its own rate on the member.
/// </summary>
/// <remarks>
/// The values are persisted, so new options are appended rather than inserted in risk order. The
/// order to show them in is a presentation concern and belongs at the point of display.
/// </remarks>
public enum GrowthStrategy : byte
{
    Custom = 0,
    Conservative = 1,
    Balanced = 2,
    Growth = 3,
    HighGrowth = 4,
    Cash = 5,
    Moderate = 6,
}
