using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[PrimaryKey(nameof(Id))]
public class ForecastPlannedItem(Guid id) : KeyedEntity<Guid>(id)
{
    public ForecastPlannedItem() : this(Guid.Empty) { }

    public Guid ForecastPlanId { get; set; }

    [ForeignKey(nameof(ForecastPlanId))]
    public virtual ForecastPlan ForecastPlan { get; set; } = null!;

    public PlannedItemType ItemType { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public int? TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    public virtual Tag.Tag? Tag { get; set; }

    public Guid? VirtualInstrumentId { get; set; }

    [ForeignKey(nameof(VirtualInstrumentId))]
    public virtual Account.VirtualInstrument? VirtualInstrument { get; set; }

    public bool IsIncluded { get; set; } = true;

    public PlannedItemDateMode DateMode { get; set; }

    // Fixed date fields
    public DateOnly? FixedDate { get; set; }

    // Schedule fields
    public ScheduleFrequency? ScheduleFrequency { get; set; }
    public DateOnly? ScheduleAnchorDate { get; set; }
    public int? ScheduleInterval { get; set; }
    public int? ScheduleDayOfMonth { get; set; }
    public DateOnly? ScheduleEndDate { get; set; }

    // Flexible window fields (V1)
    public DateOnly? WindowStartDate { get; set; }
    public DateOnly? WindowEndDate { get; set; }
    public AllocationMode? AllocationMode { get; set; }

    public string? Notes { get; set; }
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
