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

}
