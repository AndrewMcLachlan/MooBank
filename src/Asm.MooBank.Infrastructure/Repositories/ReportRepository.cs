using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class ReportRepository(MooBankContext mooBankContext) : IReportRepository
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
        var result = new Dictionary<Guid, IEnumerable<CreditDebitTotal>>();

        foreach (var accountId in accountIds)
        {
            var totals = await GetCreditDebitTotals(accountId, startDate, endDate, cancellationToken);
            result[accountId] = totals;
        }

        return result;
    }

    public async Task<Dictionary<Guid, IEnumerable<MonthlyBalance>>> GetMonthlyBalancesForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, IEnumerable<MonthlyBalance>>();

        foreach (var accountId in accountIds)
        {
            var balances = await GetMonthlyBalances(accountId, startDate, endDate, cancellationToken);
            result[accountId] = balances;
        }

        return result;
    }

    public async Task<IEnumerable<MonthlyCreditDebitTotal>> GetMonthlyCreditDebitTotals(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await mooBankContext.MonthlyCreditDebitTotals.FromSqlInterpolated($@"EXEC dbo.GetMonthlyCreditDebitTotals {accountId}, {startDate}, {endDate}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>> GetMonthlyCreditDebitTotalsForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>();

        foreach (var accountId in accountIds)
        {
            var totals = await GetMonthlyCreditDebitTotals(accountId, startDate, endDate, cancellationToken);
            result[accountId] = totals;
        }

        return result;
    }
}
