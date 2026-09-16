using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[PrimaryKey(nameof(PlannedItemId))]
public partial class PlannedItemFixedDate
{
    public Guid PlannedItemId { get; set; }

    [ForeignKey(nameof(PlannedItemId))]
    [Navigation]
    public virtual partial ForecastPlannedItem PlannedItem { get; set; }

    public required DateOnly FixedDate { get; set; }
}
