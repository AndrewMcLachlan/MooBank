﻿using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetInOutReport : ReportQuery, IQuery<InOutReport>;

internal class GetInOutReportHandler(IQueryable<Transaction> transactions, ISecurity security) : IQueryHandler<GetInOutReport, InOutReport>
{
    public async ValueTask<InOutReport> Handle(GetInOutReport request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var results = await transactions.Include(t => t.Splits).ThenInclude(t => t.OffsetBy).Include(t => t.OffsetFor).WhereByReportQuery(request)
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
            Income = results.Where(t => t.TransactionType.IsCredit()).Sum(t => t.Amount),
            Outgoings = results.Where(t => t.TransactionType.IsDebit()).Sum(t => t.Amount),
        };
    }
}
