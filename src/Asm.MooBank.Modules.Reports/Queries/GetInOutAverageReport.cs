using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetInOutAverageReport : ReportQuery, IQuery<InOutReport>;

internal class GetInOutAverageReportHandler(IQueryable<Transaction> transactions, ISecurity security) : IQueryHandler<GetInOutAverageReport, InOutReport>
{
    public async ValueTask<InOutReport> Handle(GetInOutAverageReport request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var months = request.Start.DifferenceInMonths(request.End);
        months = months == 0 ? 1 : months;

        var results = await transactions.Specify(new IncludeSplitsAndOffsetsSpecification()).WhereByReportQuery(request)
            .ExcludeOffset()
            .GroupBy(t => t.TransactionType)
            .Select(g => new
            {
                TransactionType = g.Key,
                Amount = g.Sum(t => Transaction.TransactionNetAmount(t.Id, t.Amount))
            }).ToListAsync(cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = results.Where(t => t.TransactionType.IsCredit()).Sum(t => t.Amount) / months,
            Outgoings = results.Where(t => t.TransactionType.IsDebit()).Sum(t => t.Amount) / months,
        };
    }
}
