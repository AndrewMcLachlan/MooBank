using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Commands;

public record Create(short Year) : ICommand<Models.Budget>;

internal class CreateHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, User user) :  ICommandHandler<Create, Models.Budget>
{
    public async ValueTask<Models.Budget> Handle(Create request, CancellationToken cancellationToken)
    {
        // Security: Check not required as "year" is the only user input.

        var entity = budgetRepository.Add(new Domain.Entities.Budget.Budget(Guid.Empty)
        {
            FamilyId = user.FamilyId,
            Year = request.Year,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
