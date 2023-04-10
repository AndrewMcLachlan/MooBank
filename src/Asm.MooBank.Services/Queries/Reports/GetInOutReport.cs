using Asm.Cqrs.Queries;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Reports;

internal class GetInOutReport : IQueryHandler<Models.Queries.Reports.GetInOutReport, InOutReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurityRepository _securityRepository;

    public GetInOutReport(IQueryable<Transaction> transactions, ISecurityRepository securityRepository)
    {
        _transactions = transactions;
        _securityRepository = securityRepository;
    }

    public async Task<InOutReport> Handle(Models.Queries.Reports.GetInOutReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        var results = await _transactions.Where(t => t.TransactionTime >= request.Start.ToStartOfDay() && t.TransactionTime <= request.End.ToEndOfDay())
            .GroupBy(t => t.TransactionType)
            .Select(g => new
            {
                TransactionType = g.Key,
                Amount = g.Sum(t => t.Amount)
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
