using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Services.Commands.Budget;

public record Delete(Guid AccountId, Guid Id) : ICommand;

internal class DeleteHandler : CommandHandlerBase, ICommandHandler<Delete>
{
    private readonly IBudgetRepository _budgetRepository;

    public DeleteHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, ISecurity security) : base(unitOfWork, security)
    {
        _budgetRepository = budgetRepository;
    }

    public Task Handle(Delete request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        _budgetRepository.Delete(request.Id);

        return UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
