using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Services.Commands.Budget;

public record Update(Guid AccountId, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class UpdateHandler : CommandHandlerBase, ICommandHandler<Update, Models.BudgetLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public UpdateHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, ISecurity security) : base(unitOfWork, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<Models.BudgetLine> Handle(Update request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = await _budgetRepository.Get(request.BudgetLine.Id, cancellationToken);

        entity.Amount = request.BudgetLine.Amount;
        entity.TagId = request.BudgetLine.TagId;
        entity.Month = request.BudgetLine.Month;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
