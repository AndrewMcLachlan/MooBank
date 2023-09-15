using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Queries.Transactions;

public record Search(Guid AccountId, DateOnly StartDate, Models.TransactionType TransactionType, IEnumerable<int> TagIds) : IQuery<IEnumerable<Models.Transaction>>;

internal class SearchHandler : IQueryHandler<Search, IEnumerable<Models.Transaction>>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurity _securityRepository;

    public SearchHandler(IQueryable<Transaction> transactions, ISecurity securityRepository)
    {
        _transactions = transactions;
        _securityRepository = securityRepository;
    }

    public Task<IEnumerable<Models.Transaction>> Handle(Search request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        // Sometimes rebates are accounted prior to the debit transaction.
        // Go back 5 days, just in case.
        var startTime = request.StartDate.AddDays(-5).ToStartOfDay();

        return _transactions.IncludeAll().Where<Transaction>(t => t.AccountId == request.AccountId && t.TransactionTime >= startTime && t.TransactionType == request.TransactionType && t.TransactionSplits.SelectMany(ts => ts.Tags).Any(tt => request.TagIds.Contains(tt.Id))).ToModelAsync(cancellationToken: cancellationToken);
    }
}
