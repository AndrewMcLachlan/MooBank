using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Budget.Commands;

public record DeleteLine(short Year, Guid Id) : ICommand;

internal class DeleteLineHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<DeleteLine>
{
    public async ValueTask Handle(DeleteLine request, CancellationToken cancellationToken)
    {
        await Security.AssertBudgetLinePermission(request.Id, cancellationToken);

        budgetRepository.DeleteLine(request.Id);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
