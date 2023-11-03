using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Budget.Commands;

public record CreateLine(short Year, [FromBody]Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class CreateLineHandler : CommandHandlerBase, ICommandHandler<CreateLine, Models.BudgetLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public CreateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async ValueTask<Models.BudgetLine> Handle(CreateLine request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out short year, out Models.BudgetLine budgetLine);

        var budget = await _budgetRepository.GetOrCreate(AccountHolder.FamilyId, year, cancellationToken);

        var entity = budgetLine.ToDomain(budget.Id);

        _budgetRepository.AddLine(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);


        return entity;
    }
}
