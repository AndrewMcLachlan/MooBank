using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Budget.Commands;

public record DeleteLine(short Year, Guid Id) : ICommand;

internal class DeleteLineHandler : CommandHandlerBase, ICommandHandler<DeleteLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public DeleteLineHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async ValueTask Handle(DeleteLine request, CancellationToken cancellationToken)
    {
        await Security.AssertBudgetLinePermission(request.Id, cancellationToken);

        _budgetRepository.DeleteLine(request.Id);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
