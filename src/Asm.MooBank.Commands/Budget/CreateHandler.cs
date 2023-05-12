using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Services.Commands.Budget;

public record Create(Guid AccountId, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class CreateHandler : CommandHandlerBase, ICommandHandler<Create, Models.BudgetLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public CreateHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, ISecurity security) : base(unitOfWork, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<Models.BudgetLine> Handle(Create request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = _budgetRepository.Add(request.BudgetLine.ToDomain(request.AccountId));

        var tag = entity.Tag;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
