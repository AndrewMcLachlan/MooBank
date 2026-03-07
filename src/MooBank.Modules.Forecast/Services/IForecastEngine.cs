using Asm.MooBank.Modules.Forecast.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;

namespace Asm.MooBank.Modules.Forecast.Services;

public interface IForecastEngine
{
    Task<ForecastResult> Calculate(DomainForecastPlan plan, CancellationToken cancellationToken = default);
}
