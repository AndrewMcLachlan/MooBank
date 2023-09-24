using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Queries.Reports;

public record GetInOutReport : ReportQuery, IQuery<InOutReport>;

internal class GetInOutReportHandler : IQueryHandler<GetInOutReport, InOutReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurity _securityRepository;

    public GetInOutReportHandler(IQueryable<Transaction> transactions, ISecurity securityRepository)
    {
        _transactions = transactions;
        _securityRepository = securityRepository;
    }

    public async Task<InOutReport> Handle(GetInOutReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        var results = await _transactions.Include(t => t.Splits).ThenInclude(t => t.OffsetBy).Include(t => t.OffsetFor).WhereByReportQuery(request)
            .ExcludeOffset()
            .GroupBy(t => t.TransactionType)
            .Select(g => new
            {
                TransactionType = g.Key,
                Amount = g.Sum(t => t.NetAmount)
            }).ToListAsync(cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = results.Where(t => t.TransactionType.IsCredit()).Sum(t => t.Amount),
            Outgoings = results.Where(t => t.TransactionType.IsDebit()).Sum(t => t.Amount),
        };
    }
}
