using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;

namespace Asm.MooBank.Modules.Transactions.Queries.Transactions;

public record Search(Guid AccountId, DateOnly Start, MooBank.Models.TransactionType TransactionType, int[] TagIds) : IQuery<IEnumerable<Models.Transaction>>;

internal class SearchHandler(IQueryable<Transaction> transactions, ISecurity securityRepository) : IQueryHandler<Search, IEnumerable<Models.Transaction>>
{
    public async ValueTask<IEnumerable<Models.Transaction>> Handle(Search request, CancellationToken cancellationToken)
    {
        securityRepository.AssertInstrumentPermission(request.AccountId);

        // Sometimes rebates are accounted prior to the debit transaction.
        // Go back 5 days, just in case.
        var startTime = request.Start.AddDays(-5).ToStartOfDay();

        return await transactions.IncludeAll().Where(t => t.AccountId == request.AccountId && t.TransactionTime >= startTime && t.TransactionType == request.TransactionType && t.Splits.SelectMany(ts => ts.Tags).Any(tt => request.TagIds.Contains(tt.Id))).ToModel().ToListAsync(cancellationToken);
    }
}
