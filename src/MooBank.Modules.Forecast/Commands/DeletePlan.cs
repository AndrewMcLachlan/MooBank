using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("DeleteForecastPlan")]
public record DeletePlan(Guid Id) : ICommand;

internal class DeletePlanHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork) : ICommandHandler<DeletePlan>
{
    public async ValueTask Handle(DeletePlan request, CancellationToken cancellationToken)
    {
        var entity = await forecastRepository.Get(request.Id, cancellationToken);

        entity.Archive();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
