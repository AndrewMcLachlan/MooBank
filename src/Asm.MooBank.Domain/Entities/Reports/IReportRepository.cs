using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Reports;

public interface IReportRepository
{
    Task<IEnumerable<TransactionTagTotal>> GetTransactionTagTotals(Guid accountId, DateOnly startDate, DateOnly endDate, TransactionFilterType filterType, int? rootTagId = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<MonthlyTagTotal>> GetMonthlyTotalsForTag(Guid accountId, DateOnly startDate, DateOnly endDate, TransactionFilterType filterType, int? tagId = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<CreditDebitTotal>> GetCreditDebitTotals(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<CreditDebitAverage>> GetCreditDebitAverages(Guid accountId, DateOnly startDate, DateOnly endDate, ReportInterval interval, CancellationToken cancellationToken = default);

    Task<IEnumerable<TagAverage>> GetTopTagAverages(Guid accountId, DateOnly startDate, DateOnly endDate, ReportInterval interval, CancellationToken cancellationToken = default);

    Task<IEnumerable<MonthlyBalance>> GetMonthlyBalances(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<MonthlyBalance>> GetGroupMonthlyBalances(Guid groupId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, IEnumerable<CreditDebitTotal>>> GetCreditDebitTotalsForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, IEnumerable<MonthlyBalance>>> GetMonthlyBalancesForAccounts(IEnumerable<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}

