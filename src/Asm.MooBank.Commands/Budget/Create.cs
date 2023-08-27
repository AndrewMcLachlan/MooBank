using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Services.Commands;

namespace Asm.MooBank.Commands.Budget;

public record Create(short Year) : ICommand<Models.Budget>;
internal class CreateHandler : CommandHandlerBase, ICommandHandler<Create, Models.Budget>
{
    private readonly IBudgetRepository _budgetRepository;

    public CreateHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, ISecurity security) : base(unitOfWork, security)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<Models.Budget> Handle(Create request, CancellationToken cancellationToken)
    {
        var familyId = await Security.GetFamilyId();

        var entity = _budgetRepository.Add(new Domain.Entities.Budget.Budget(Guid.Empty)
        {
            FamilyId = familyId,
            Year = request.Year,
        });

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
