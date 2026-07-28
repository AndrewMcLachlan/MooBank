using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Commands;

[DisplayName("DeleteRetirementPlan")]
public record DeletePlan(Guid Id) : ICommand;

internal class DeletePlanHandler(IRetirementRepository retirementRepository, IUnitOfWork unitOfWork) : ICommandHandler<DeletePlan>
{
    public async ValueTask Handle(DeletePlan request, CancellationToken cancellationToken)
    {
        var entity = await retirementRepository.Get(request.Id, cancellationToken);

        retirementRepository.Delete(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
