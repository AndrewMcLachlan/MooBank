using System.ComponentModel;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Forecast.Models;

[DisplayName("PlannedItem")]
public sealed record PlannedItem
{
    public Guid Id { get; init; }
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

public enum PlannedItemType : byte
{
    Expense = 0,
    Income = 1
}

public enum PlannedItemDateMode : byte
{
    FixedDate = 0,
    Schedule = 1,
    FlexibleWindow = 2
}

public enum AllocationMode : byte
{
    EvenlySpread = 0,
    AllAtEnd = 1
}
