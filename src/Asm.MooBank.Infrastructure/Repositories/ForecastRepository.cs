using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class ForecastRepository(MooBankContext context) : RepositoryWriteBase<MooBankContext, ForecastPlan, Guid>(context), IForecastRepository
{
}
