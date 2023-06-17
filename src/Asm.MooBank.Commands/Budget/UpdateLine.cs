using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Services.Commands.Budget;

public record UpdateLine(Guid AccountId, short Year, Guid Id, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class UpdateLineHandler : CommandHandlerBase, ICommandHandler<UpdateLine, Models.BudgetLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public UpdateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, ISecurity security) : base(unitOfWork, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<Models.BudgetLine> Handle(UpdateLine request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var budget = await _budgetRepository.GetByYear(request.AccountId, request.Year, cancellationToken);

        var entity = budget.Lines.Single(b => b.Id == request.Id);

        entity.Amount = request.BudgetLine.Amount;
        entity.TagId = request.BudgetLine.TagId;
        entity.Month = request.BudgetLine.Month;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
