using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Services.Commands.Budget;

public record CreateLine(short Year, Models.BudgetLine BudgetLine) : ICommand<Models.BudgetLine>;

internal class CreateLineHandler : CommandHandlerBase, ICommandHandler<CreateLine, Models.BudgetLine>
{
    private readonly IBudgetRepository _budgetRepository;

    public CreateLineHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, ISecurity security) : base(unitOfWork, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<Models.BudgetLine> Handle(CreateLine request, CancellationToken cancellationToken)
    {
        request.Deconstruct( out short year, out Models.BudgetLine budgetLine);

        var familyId = await Security.GetFamilyId();

        var budget = await _budgetRepository.GetOrCreate(familyId, year, cancellationToken);

        var entity = budgetLine.ToDomain(budget.Id);

        _budgetRepository.AddLine(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);


        return entity;
    }
}
