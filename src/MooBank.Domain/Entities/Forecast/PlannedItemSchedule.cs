using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[PrimaryKey(nameof(PlannedItemId))]
public partial class PlannedItemSchedule
{
    public Guid PlannedItemId { get; set; }

    [ForeignKey(nameof(PlannedItemId))]
    [Navigation]
    public virtual partial ForecastPlannedItem PlannedItem { get; set; }

    public ScheduleFrequency Frequency { get; set; }

    public required DateOnly AnchorDate { get; set; }

    public int Interval { get; set; } = 1;

    public int? DayOfMonth { get; set; }

    public DateOnly? EndDate { get; set; }
}
