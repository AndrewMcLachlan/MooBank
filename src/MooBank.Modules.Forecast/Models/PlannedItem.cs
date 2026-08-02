using System.ComponentModel;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Forecast.Models;

[DisplayName("SimplePlannedItem")]
public record PlannedItemBase
{
    public PlannedItemType ItemType { get; init; }
    public required string Name { get; init; }
    public decimal Amount { get; init; }
    public int? TagId { get; init; }
    public string? TagName { get; init; }
    public Guid? VirtualInstrumentId { get; init; }
    public bool IsIncluded { get; init; }
    public PlannedItemDateMode DateMode { get; init; }

    // Fixed date
    public DateOnly? FixedDate { get; init; }

    // Schedule
    public ScheduleFrequency? ScheduleFrequency { get; init; }
    public DateOnly? ScheduleAnchorDate { get; init; }
    public int? ScheduleInterval { get; init; }
    public int? ScheduleDayOfMonth { get; init; }
    public DateOnly? ScheduleEndDate { get; init; }

    // Flexible window (V1)
    public DateOnly? WindowStartDate { get; init; }
    public DateOnly? WindowEndDate { get; init; }
    public AllocationMode? AllocationMode { get; init; }

    public string? Notes { get; init; }
}

/// <summary>
/// A payment that could belong to a planned item, offered for the author to confirm.
/// </summary>
[DisplayName("PaymentCandidate")]
public sealed record PaymentCandidate
{
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
    public DateOnly When { get; init; }
    public string? Description { get; init; }

    /// <summary>A positive magnitude, net of offsets.</summary>
    public decimal Amount { get; init; }

    /// <summary>Whether this payment is already linked to the item.</summary>
    public bool IsLinked { get; init; }
}

[DisplayName("PlannedItem")]
public sealed record PlannedItem : PlannedItemBase
{
    public Guid Id { get; init; }

    /// <summary>
    /// Payments the author has said belong to this item. Where there are any, they are the item's
    /// actuals and the tag plays no further part.
    /// </summary>
    public IEnumerable<Guid> LinkedTransactionIds { get; init; } = [];
}
