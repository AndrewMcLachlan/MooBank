using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Modules.Forecast.Models;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("ArchiveForecastPlan")]
public record ArchivePlan(Guid Id) : ICommand<Models.ForecastPlan>;

internal class ArchivePlanHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<ArchivePlan, Models.ForecastPlan>
{
    public async ValueTask<Models.ForecastPlan> Handle(ArchivePlan request, CancellationToken cancellationToken)
    {
        var entity = await forecastRepository.Get(request.Id, cancellationToken);
        await security.AssertFamilyPermission(entity.FamilyId);

        entity.Archive();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
