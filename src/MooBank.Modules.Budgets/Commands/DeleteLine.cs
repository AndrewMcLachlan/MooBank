using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Budgets.Commands;

public record DeleteLine(short Year, Guid Id) : ICommand;

internal class DeleteLineHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<DeleteLine>
{
    public async ValueTask Handle(DeleteLine request, CancellationToken cancellationToken)
    {
        await security.AssertBudgetLinePermission(request.Id, cancellationToken);

        budgetRepository.DeleteLine(request.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
