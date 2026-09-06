namespace Asm.MooBank.Modules.Bills.Models;

/// <summary>
/// The details a bill is being changed to.
/// </summary>
/// <remarks>
/// The periods supplied replace those on the bill, so a caller adding an export sends the
/// consumption alongside it rather than the export alone.
/// </remarks>
public record UpdateBill : BillBase
{
}
