using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Commands.Budget;

public record Create(short Year) : ICommand<Models.Budget>;
internal class CreateHandler : CommandHandlerBase, ICommandHandler<Create, Models.Budget>
{
    private readonly IBudgetRepository _budgetRepository;

    public CreateHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<Models.Budget> Handle(Create request, CancellationToken cancellationToken)
    {
        var entity = _budgetRepository.Add(new Domain.Entities.Budget.Budget(Guid.Empty)
        {
            FamilyId = AccountHolder.FamilyId,
            Year = request.Year,
        });

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
