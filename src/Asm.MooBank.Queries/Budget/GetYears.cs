namespace Asm.MooBank.Queries.Budget;

public record GetYears(Guid AccountId) : IQuery<IEnumerable<short>>;

internal class GetYearsHandler : QueryHandlerBase, IQueryHandler<GetYears, IEnumerable<short>>
{

    private readonly IQueryable<Domain.Entities.Budget.Budget> _budgets;

    public GetYearsHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, ISecurity security) : base(security)
    {
        _budgets = budgets;
    }

    public async Task<IEnumerable<short>> Handle(GetYears request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        return await _budgets.Where(b => b.AccountId == request.AccountId).Select(b => b.Year).ToListAsync(cancellationToken: cancellationToken);
    }
}
