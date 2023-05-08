using Asm.Domain;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models.Commands.Budget;

namespace Asm.MooBank.Services.Commands.Budget;

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
