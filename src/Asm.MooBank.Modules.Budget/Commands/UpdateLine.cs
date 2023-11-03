using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;

namespace Asm.MooBank.Modules.Budget.Commands;

public record UpdateLine(short Year, Guid Id, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class UpdateLineHandler : CommandHandlerBase, ICommandHandler<UpdateLine, Models.BudgetLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public UpdateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async ValueTask<Models.BudgetLine> Handle(UpdateLine request, CancellationToken cancellationToken)
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
