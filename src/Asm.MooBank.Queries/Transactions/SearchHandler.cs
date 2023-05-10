using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Queries.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Queries.Transactions;

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

        var startTime = request.StartDate.ToStartOfDay();

        return _transactions.IncludeAll().Where<Transaction>(t => t.AccountId == request.AccountId && t.Offsets == null && t.TransactionTime > startTime && t.TransactionType == request.TransactionType && t.TransactionTags.Any(tt => request.TagIds.Contains(tt.TransactionTagId))).ToModelAsync(cancellationToken: cancellationToken);
    }
}
