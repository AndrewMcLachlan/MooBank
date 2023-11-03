using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Budget.Queries;

public record Get(short Year) : IQuery<Models.Budget?>;

internal class GetHandler : QueryHandlerBase, IQueryHandler<Get, Models.Budget?>
{
    private readonly IQueryable<Domain.Entities.Budget.Budget> _budgets;

    public GetHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, AccountHolder accountHolder) : base(accountHolder)
    {
        _budgets = budgets;
    }

    public async ValueTask<Models.Budget?> Handle(Get request, CancellationToken cancellationToken)
    {
        var familyId = AccountHolder.FamilyId;

        return await _budgets.Include(b => b.Lines).ThenInclude(b => b.Tag).Where(b => b.FamilyId == familyId && b.Year == request.Year).SingleOrDefaultAsync(cancellationToken);
    }
}
