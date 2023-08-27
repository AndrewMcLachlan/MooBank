namespace Asm.MooBank.Queries.Budget;

public record Get(short Year) : IQuery<Models.Budget?>;

internal class GetHandler : QueryHandlerBase, IQueryHandler<Get, Models.Budget?>
{
    private readonly IQueryable<Domain.Entities.Budget.Budget> _budgets;

    public GetHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, ISecurity security) : base(security)
    {
        _budgets = budgets;
    }

    public async Task<Models.Budget?> Handle(Get request, CancellationToken cancellationToken)
    {
        var familyId = await Security.GetFamilyId();

        return await _budgets.Include(b => b.Lines).ThenInclude(b => b.Tag).Where(b => b.FamilyId == familyId && b.Year == request.Year).SingleOrDefaultAsync(cancellationToken);
    }
}
