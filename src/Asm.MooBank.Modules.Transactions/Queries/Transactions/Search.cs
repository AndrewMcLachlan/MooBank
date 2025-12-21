using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Modules.Transactions.Models.Extensions;

namespace Asm.MooBank.Modules.Transactions.Queries.Transactions;

public record Search(Guid InstrumentId, DateOnly Start, MooBank.Models.TransactionType TransactionType, int[] TagIds) : IQuery<IEnumerable<MooBank.Models.Transaction>>;

internal class SearchHandler(IQueryable<Transaction> transactions) : IQueryHandler<Search, IEnumerable<MooBank.Models.Transaction>>
{
    public async ValueTask<IEnumerable<MooBank.Models.Transaction>> Handle(Search request, CancellationToken cancellationToken)
    {
        // Sometimes rebates are accounted prior to the debit transaction.
        // Go back 5 days, just in case.
        var startTime = request.Start.AddDays(-5).ToStartOfDay();

        return await transactions.Specify(new IncludeAllSpecification()).Where(t => t.AccountId == request.InstrumentId && t.TransactionTime >= startTime && t.TransactionType == request.TransactionType && t.Splits.SelectMany(ts => ts.Tags).Any(tt => request.TagIds.Contains(tt.Id))).OrderBy(t => t.TransactionTime).ToModel().ToListAsync(cancellationToken);
    }
}
