using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;

namespace Asm.MooBank.Modules.Budget.Commands;

public record Create(short Year) : ICommand<Models.Budget>;

internal class CreateHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, User accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Budget>
{
    public async ValueTask<Models.Budget> Handle(Create request, CancellationToken cancellationToken)
    {
        var entity = budgetRepository.Add(new Domain.Entities.Budget.Budget(Guid.Empty)
        {
            FamilyId = AccountHolder.FamilyId,
            Year = request.Year,
        });

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
