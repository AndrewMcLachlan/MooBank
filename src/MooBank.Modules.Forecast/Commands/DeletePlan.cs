using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("DeleteForecastPlan")]
public record DeletePlan(Guid Id) : ICommand;

internal class DeletePlanHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<DeletePlan>
{
    public async ValueTask Handle(DeletePlan request, CancellationToken cancellationToken)
    {
        var entity = await forecastRepository.Get(request.Id, cancellationToken);
        await security.AssertFamilyPermission(entity.FamilyId);

        entity.Archive();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
