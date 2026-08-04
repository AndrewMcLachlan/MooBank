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
    public virtual Instrument.VirtualInstrument? VirtualInstrument { get; set; }

    public bool IsIncluded { get; set; } = true;

    public PlannedItemDateMode DateMode { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Payments the author has said belong to this item. Where there are any, they are the item's
    /// actuals and tag matching plays no further part.
    /// </summary>
    public virtual ICollection<ForecastPlannedItemTransaction> Transactions { get; set; } = [];

    // Navigation properties for schedule configurations (0-1 relationship)
    public virtual PlannedItemFixedDate? FixedDate { get; set; }
    public virtual PlannedItemSchedule? Schedule { get; set; }
}
