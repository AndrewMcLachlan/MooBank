using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Models.Recurring;

/// <summary>
/// The mutable fields of a recurring transaction. The instrument, virtual instrument and
/// recurring transaction ids identify the resource and come from the route, so they are
/// deliberately absent here.
/// </summary>
public record RecurringTransactionDetails
{
    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public ScheduleFrequency Schedule { get; set; }

    public DateOnly NextRun { get; set; }
}
