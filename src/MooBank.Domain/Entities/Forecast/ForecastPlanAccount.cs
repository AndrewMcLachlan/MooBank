using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[PrimaryKey(nameof(Id))]
public class ForecastPlanAccount(Guid id) : KeyedEntity<Guid>(id)
{
    public ForecastPlanAccount() : this(Guid.Empty) { }

    public Guid ForecastPlanId { get; set; }

    [ForeignKey(nameof(ForecastPlanId))]
    public virtual ForecastPlan ForecastPlan { get; set; } = null!;

    public Guid InstrumentId { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    public virtual Instrument.Instrument Instrument { get; set; } = null!;
}
