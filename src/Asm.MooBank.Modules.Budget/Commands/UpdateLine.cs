using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Commands;

public record UpdateLine(short Year, Guid Id, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class UpdateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<UpdateLine, Models.BudgetLine>
{
    public async ValueTask<Models.BudgetLine> Handle(UpdateLine request, CancellationToken cancellationToken)
    {
        await Security.AssertBudgetLinePermission(request.Id, cancellationToken);

        var budget = await budgetRepository.GetByYear(AccountHolder.FamilyId, request.Year, cancellationToken);

        var entity = budget.Lines.Single(b => b.Id == request.Id);

        entity.Amount = request.BudgetLine.Amount;
        entity.TagId = request.BudgetLine.TagId;
        entity.Month = request.BudgetLine.Month;
        entity.Notes = request.BudgetLine.Notes;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
