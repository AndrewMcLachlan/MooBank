using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetInOutAverageReport : ReportQuery, IQuery<InOutReport>
{
    public ReportInterval Interval { get; init; } = ReportInterval.Monthly;
}

internal class GetInOutAverageReportHandler(IQueryable<Transaction> transactions) : IQueryHandler<GetInOutAverageReport, InOutReport>
{
    public async ValueTask<InOutReport> Handle(GetInOutAverageReport request, CancellationToken cancellationToken)
    {
        var intervals = request.Interval == ReportInterval.Monthly ? request.Start.DifferenceInMonths(request.End) : request.End.Year - request.Start.Year;
        intervals = intervals == 0 ? 1 : intervals;

        var results = await transactions.Specify(new IncludeSplitsAndOffsetsSpecification()).WhereByReportQuery(request)
            .ExcludeOffset()
            .GroupBy(t => t.TransactionType)
            .Select(g => new
            {
                TransactionType = g.Key,
                Amount = g.Sum(t => Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount))
            }).ToListAsync(cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = results.Where(t => t.TransactionType.IsCredit()).Sum(t => t.Amount) / intervals,
            Outgoings = results.Where(t => t.TransactionType.IsDebit()).Sum(t => t.Amount) / intervals,
        };
    }
}
