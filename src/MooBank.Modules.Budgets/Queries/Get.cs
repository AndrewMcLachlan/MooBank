using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record Get(short Year) : IQuery<Budget?>;

internal class GetHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, User user, ICommandDispatcher commandDispatcher) : IQueryHandler<Get, Budget?>
{
    public async ValueTask<Budget?> Handle(Get query, CancellationToken cancellationToken)
    {
        var familyId = user.FamilyId;

        var entity = await budgets.Include(b => b.Lines).ThenInclude(b => b.Tag).Where(b => b.FamilyId == familyId && b.Year == query.Year).SingleOrDefaultAsync(cancellationToken);

        if (entity != null) return entity.ToModel();

        // Create the budget if it does not exist
        return await commandDispatcher.Dispatch(new Create(query.Year), cancellationToken);
    }
}
