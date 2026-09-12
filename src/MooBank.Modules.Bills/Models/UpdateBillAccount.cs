namespace Asm.MooBank.Modules.Bills.Models;

/// <summary>
/// The details a utility account is being changed to.
/// </summary>
/// <remarks>
/// The utility type and the currency are fixed at creation: the bills already held are recorded
/// against both, and only read correctly against them.
/// </remarks>
public record UpdateBillAccount
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public required string AccountNumber { get; init; }

    public bool ShareWithFamily { get; init; }
}
