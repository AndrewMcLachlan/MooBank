using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;
internal class ReportRepository(MooBankContext mooBankContext) : IReportRepository
{
    public async Task<IEnumerable<TransactionTagTotal>> GetTransactionTagTotals(Guid accountId, DateOnly startDate, DateOnly endDate, TransactionFilterType filterType, int? rootTagId = null, CancellationToken cancellationToken = default) =>
        await mooBankContext.TransactionTagTotals.FromSqlInterpolated($@"EXEC dbo.GetTransactionTotalsByTag {accountId}, {startDate}, {endDate}, {rootTagId}, {filterType.ToString()}").AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<MonthlyTagTotal>> GetMonthlyTotalsForTag(Guid accountId, DateOnly startDate, DateOnly endDate, TransactionFilterType filterType, int? tagId = null, CancellationToken cancellationToken = default) =>
            await mooBankContext.MonthlyTagTotals.FromSqlInterpolated($@"EXEC dbo.GetMonthlyTotalsForTag {accountId}, {startDate}, {endDate}, {tagId}, {filterType.ToString()}").AsNoTracking().ToListAsync(cancellationToken);


}
