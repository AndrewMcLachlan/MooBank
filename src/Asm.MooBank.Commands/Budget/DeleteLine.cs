using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Commands.Budget;

public record DeleteLine(short Year, Guid Id) : ICommand;

internal class DeleteLineHandler : CommandHandlerBase, ICommandHandler<DeleteLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public DeleteLineHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _budgetRepository = budgetRepository;
    }

    public Task Handle(DeleteLine request, CancellationToken cancellationToken)
    {
        Security.AssertBudgetLinePermission(request.Id);

        _budgetRepository.DeleteLine(request.Id);

        return UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
