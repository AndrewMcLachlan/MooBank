using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[PrimaryKey(nameof(Id))]
public partial class ForecastPlanAccount(Guid id) : KeyedEntity<Guid>(id)
{
    public ForecastPlanAccount() : this(Guid.Empty) { }

    public Guid ForecastPlanId { get; set; }

    [ForeignKey(nameof(ForecastPlanId))]
    [Navigation]
    public virtual partial ForecastPlan ForecastPlan { get; set; }

    public Guid InstrumentId { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    [Navigation]
    public virtual partial Instrument.Instrument Instrument { get; set; }
}
