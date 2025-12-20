using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[PrimaryKey(nameof(PlannedItemId))]
public class PlannedItemFixedDate
{
    public Guid PlannedItemId { get; set; }

    [ForeignKey(nameof(PlannedItemId))]
    public virtual ForecastPlannedItem PlannedItem { get; set; } = null!;

    public required DateOnly FixedDate { get; set; }
}
