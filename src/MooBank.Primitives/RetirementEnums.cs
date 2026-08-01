namespace Asm.MooBank;

/// <summary>
/// The investment option a superannuation balance is invested in. Each named strategy carries an
/// assumed long-run return; <see cref="Custom"/> defers to the rate set on the plan.
/// </summary>
public enum GrowthStrategy : byte
{
    Custom = 0,
    Conservative = 1,
    Balanced = 2,
    Growth = 3,
    HighGrowth = 4,
}
