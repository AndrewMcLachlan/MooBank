using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[PrimaryKey(nameof(PlannedItemId))]
public class PlannedItemFlexibleWindow
{
    public Guid PlannedItemId { get; set; }

    [ForeignKey(nameof(PlannedItemId))]
    public virtual ForecastPlannedItem PlannedItem { get; set; } = null!;

    public required DateOnly StartDate { get; set; }

    public required DateOnly EndDate { get; set; }

    public AllocationMode AllocationMode { get; set; }
}
