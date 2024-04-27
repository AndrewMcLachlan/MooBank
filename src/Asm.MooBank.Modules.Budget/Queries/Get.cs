using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Commands;
using Asm.MooBank.Modules.Budget.Models;

namespace Asm.MooBank.Modules.Budget.Queries;

public record Get(short Year) : IQuery<Models.Budget?>;

internal class GetHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, User accountHolder, ICommandDispatcher commandDispatcher) : IQueryHandler<Get, Models.Budget?>
{
    public async ValueTask<Models.Budget?> Handle(Get query, CancellationToken cancellationToken)
    {
        var familyId = accountHolder.FamilyId;

        var entity = await budgets.Include(b => b.Lines).ThenInclude(b => b.Tag).Where(b => b.FamilyId == familyId && b.Year == query.Year).SingleOrDefaultAsync(cancellationToken);

        if (entity != null) return entity.ToModel();

        // Create the budget if it does not exist
        return await commandDispatcher.Dispatch(new Create(query.Year), cancellationToken);
    }
}
