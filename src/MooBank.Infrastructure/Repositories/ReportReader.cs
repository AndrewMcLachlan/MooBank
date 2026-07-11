using System.Data;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Microsoft.Data.SqlClient;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class ReportReader(MooBankContext mooBankContext) : IReportReader
{
    public async Task<IEnumerable<TransactionTagTotal>> GetTransactionTagTotals(Guid accountId, DateOnly startDate, DateOnly endDate, TransactionFilterType filterType, int? rootTagId = null, CancellationToken cancellationToken = default) =>
        await mooBankContext.TransactionTagTotals.FromSqlInterpolated($@"EXEC dbo.GetTransactionTotalsByTag {accountId}, {startDate}, {endDate}, {rootTagId}, {(int)filterType}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<MonthlyTagTotal>> GetMonthlyTotalsForTag(Guid accountId, DateOnly startDate, DateOnly endDate, TransactionFilterType filterType, int? tagId = null, CancellationToken cancellationToken = default) =>
            await mooBankContext.MonthlyTagTotals.FromSqlInterpolated($@"EXEC dbo.GetMonthlyTotalsForTag {accountId}, {startDate}, {endDate}, {tagId}, {(int)filterType}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<CreditDebitTotal>> GetCreditDebitTotals(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.CreditDebitTotals.FromSqlInterpolated($@"EXEC dbo.GetCreditDebitTotals {accountId}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<CreditDebitAverage>> GetCreditDebitAverages(Guid accountId, DateOnly startDate, DateOnly endDate, ReportInterval interval, CancellationToken cancellationToken = default) =>
        await mooBankContext.CreditDebitAverages.FromSqlInterpolated($@"EXEC dbo.GetCreditDebitAverages {accountId}, {startDate}, {endDate}, {interval.ToString()}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<TagAverage>> GetTopTagAverages(Guid accountId, DateOnly startDate, DateOnly endDate, ReportInterval interval, CancellationToken cancellationToken = default) =>
        await mooBankContext.TopTagAverages.FromSqlInterpolated($@"EXEC dbo.GetTopTagAverages {accountId}, {startDate}, {endDate}, {interval.ToString()}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<MonthlyBalance>> GetMonthlyBalances(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.MonthlyBalances.FromSqlInterpolated($@"EXEC dbo.GetMonthlyBalances {accountId}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<MonthlyBalance>> GetGroupMonthlyBalances(Guid groupId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.MonthlyBalances.FromSqlInterpolated($@"EXEC dbo.GetGroupMonthlyBalances {groupId}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Dictionary<Guid, IEnumerable<CreditDebitTotal>>> GetCreditDebitTotalsForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var ids = accountIds.Distinct().ToList();
        var result = ids.ToDictionary(id => id, _ => Enumerable.Empty<CreditDebitTotal>());

        if (ids.Count == 0) return result;

        var rows = await QueryCreditDebitTotalsForAccounts(ids, startDate, endDate, cancellationToken);

        foreach (var group in rows.GroupBy(r => r.AccountId))
        {
            result[group.Key] = group.Select(r => new CreditDebitTotal
            {
                TransactionType = r.TransactionType,
                Total = r.Total,
            }).ToList();
        }

        return result;
    }

    public async Task<Dictionary<Guid, IEnumerable<MonthlyBalance>>> GetMonthlyBalancesForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var ids = accountIds.Distinct().ToList();
        var result = ids.ToDictionary(id => id, _ => Enumerable.Empty<MonthlyBalance>());

        if (ids.Count == 0) return result;

        var rows = await QueryMonthlyBalancesForAccounts(ids, startDate, endDate, cancellationToken);

        foreach (var group in rows.GroupBy(r => r.AccountId))
        {
            result[group.Key] = group.Select(r => new MonthlyBalance
            {
                PeriodEnd = r.PeriodEnd,
                Balance = r.Balance,
            }).ToList();
        }

        return result;
    }

    public async Task<IEnumerable<MonthlyCreditDebitTotal>> GetMonthlyCreditDebitTotals(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.MonthlyCreditDebitTotals.FromSqlInterpolated($@"EXEC dbo.GetMonthlyCreditDebitTotals {accountId}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>> GetMonthlyCreditDebitTotalsForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var ids = accountIds.Distinct().ToList();
        var result = ids.ToDictionary(id => id, _ => Enumerable.Empty<MonthlyCreditDebitTotal>());

        if (ids.Count == 0) return result;

        var rows = await QueryMonthlyCreditDebitTotalsForAccounts(ids, startDate, endDate, cancellationToken);

        foreach (var group in rows.GroupBy(r => r.AccountId))
        {
            result[group.Key] = group.Select(r => new MonthlyCreditDebitTotal
            {
                Month = r.Month,
                TransactionType = r.TransactionType,
                Total = r.Total,
            }).ToList();
        }

        return result;
    }

    private async Task<IEnumerable<AccountCreditDebitTotal>> QueryCreditDebitTotalsForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.AccountCreditDebitTotals.FromSqlInterpolated($@"EXEC dbo.GetCreditDebitTotalsForAccounts {CreateAccountIdsParameter(accountIds)}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    private async Task<IEnumerable<AccountMonthlyBalance>> QueryMonthlyBalancesForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.AccountMonthlyBalances.FromSqlInterpolated($@"EXEC dbo.GetMonthlyBalancesForAccounts {CreateAccountIdsParameter(accountIds)}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    private async Task<IEnumerable<AccountMonthlyCreditDebitTotal>> QueryMonthlyCreditDebitTotalsForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.AccountMonthlyCreditDebitTotals.FromSqlInterpolated($@"EXEC dbo.GetMonthlyCreditDebitTotalsForAccounts {CreateAccountIdsParameter(accountIds)}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    private static SqlParameter CreateAccountIdsParameter(IEnumerable<Guid> accountIds)
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(Guid));

        foreach (var id in accountIds)
        {
            table.Rows.Add(id);
        }

        return new SqlParameter("@AccountIds", SqlDbType.Structured)
        {
            TypeName = "dbo.GuidList",
            Value = table,
        };
    }

}
