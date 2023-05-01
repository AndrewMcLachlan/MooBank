using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Queries.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Transactions;

internal class SearchHandler : IQueryHandler<Search, IEnumerable<Models.Transaction>>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurityRepository _securityRepository;

    public SearchHandler(IQueryable<Transaction> transactions, ISecurityRepository securityRepository)
    {
        _transactions = transactions;
        _securityRepository = securityRepository;
    }

    public Task<IEnumerable<Models.Transaction>> Handle(Search request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        var startTime = request.StartDate.ToStartOfDay();

        return _transactions.IncludeAll().Where<Transaction>(t => t.AccountId == request.AccountId && t.TransactionTime > startTime && t.TransactionType == request.TransactionType && t.TransactionTags.Any(tt => request.TagIds.Contains(tt.TransactionTagId))).ToModelAsync(cancellationToken: cancellationToken);
    }
}
