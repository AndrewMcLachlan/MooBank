﻿using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Reports;

internal class GetInOutTrendReport : IQueryHandler<Models.Queries.Reports.GetInOutTrendReport, InOutTrendReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurityRepository _securityRepository;

    public GetInOutTrendReport(IQueryable<Transaction> transactions, ISecurityRepository securityRepository)
    {
        _transactions = transactions;
        _securityRepository = securityRepository;
    }

    public async Task<InOutTrendReport> Handle(Models.Queries.Reports.GetInOutTrendReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        var groupedQuery = await (
            from t in _transactions
            where !t.ExcludeFromReporting && t.TransactionTime >= request.Start.ToStartOfDay() && t.TransactionTime <= request.End.ToEndOfDay()
            group t by t.TransactionType
        ).ToListAsync(cancellationToken);

        var income = GetTrendPoints(groupedQuery.Where(g => g.Key.IsCredit()).SelectMany(g => g.AsQueryable()));
        var expenses = GetTrendPoints(groupedQuery.Where(g => g.Key.IsDebit()).SelectMany(g => g.AsQueryable()));

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = income,
            Expenses = expenses,
        };
    }

    private IEnumerable<TrendPoint> GetTrendPoints(IEnumerable<Transaction> transactions)
    {
        return transactions.GroupBy(t => new DateOnly(t.TransactionTime.Year, t.TransactionTime.Month, 1)).OrderBy(g => g.Key).Select(g => new TrendPoint
        {
            Month = g.Key,
            Amount = g.Sum(t => t.Amount)
        });
    }
}