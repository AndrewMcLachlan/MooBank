using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Commands;

public record CreateLine(short Year, Models.BudgetLineBase BudgetLine) : ICommand<Models.BudgetLine>;

internal class CreateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, User user) : ICommandHandler<CreateLine, Models.BudgetLine>
{
    public async ValueTask<Models.BudgetLine> Handle(CreateLine request, CancellationToken cancellationToken)
    {
        // Security: Check not required as "year" is the only user input, not a specific budget ID.

        request.Deconstruct(out short year, out Models.BudgetLineBase budgetLine);

        var budget = await budgetRepository.GetOrCreate(user.FamilyId, year, cancellationToken);

        var entity = budgetLine.ToDomain(budget.Id);

        budgetRepository.AddLine(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
