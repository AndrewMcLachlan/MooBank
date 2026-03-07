using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Commands;

public record UpdateLine(short Year, Guid Id, Models.BudgetLineBase BudgetLine) : ICommand<Models.BudgetLine>;

internal class UpdateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, User user, ISecurity security) : ICommandHandler<UpdateLine, Models.BudgetLine>
{
    public async ValueTask<Models.BudgetLine> Handle(UpdateLine request, CancellationToken cancellationToken)
    {
        await security.AssertBudgetLinePermission(request.Id, cancellationToken);

        var budget = await budgetRepository.GetByYear(user.FamilyId, request.Year, cancellationToken);

        var entity = budget.Lines.Single(b => b.Id == request.Id);

        entity.Amount = request.BudgetLine.Amount;
        entity.TagId = request.BudgetLine.TagId;
        entity.Month = request.BudgetLine.Month;
        entity.Notes = request.BudgetLine.Notes;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
