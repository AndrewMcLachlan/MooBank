using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Commands;

public record CreateLine(short Year, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class CreateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<CreateLine, Models.BudgetLine>
{
    public async ValueTask<Models.BudgetLine> Handle(CreateLine request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out short year, out Models.BudgetLine budgetLine);

        var budget = await budgetRepository.GetOrCreate(AccountHolder.FamilyId, year, cancellationToken);

        var entity = budgetLine.ToDomain(budget.Id);

        budgetRepository.AddLine(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
