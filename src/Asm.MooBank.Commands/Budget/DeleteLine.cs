using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Services.Commands.Budget;

public record DeleteLine(short Year, Guid Id) : ICommand;

internal class DeleteLineHandler : CommandHandlerBase, ICommandHandler<DeleteLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public DeleteLineHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, ISecurity security) : base(unitOfWork, security)
    {
        _budgetRepository = budgetRepository;
    }

    public Task Handle(DeleteLine request, CancellationToken cancellationToken)
    {
        Security.AssetBudgetLinePermission(request.Id);

        _budgetRepository.DeleteLine(request.Id);

        return UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
