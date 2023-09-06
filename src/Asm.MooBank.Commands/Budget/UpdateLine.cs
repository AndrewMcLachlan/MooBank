using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Commands.Budget;

public record UpdateLine(short Year, Guid Id, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class UpdateLineHandler : CommandHandlerBase, ICommandHandler<UpdateLine, Models.BudgetLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public UpdateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<Models.BudgetLine> Handle(UpdateLine request, CancellationToken cancellationToken)
    {
        await Security.AssertBudgetLinePermission(request.Id, cancellationToken);

        var budget = await _budgetRepository.GetByYear(AccountHolder.FamilyId, request.Year, cancellationToken);

        var entity = budget.Lines.Single(b => b.Id == request.Id);

        entity.Amount = request.BudgetLine.Amount;
        entity.TagId = request.BudgetLine.TagId;
        entity.Month = request.BudgetLine.Month;
        entity.Notes = request.BudgetLine.Notes;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
