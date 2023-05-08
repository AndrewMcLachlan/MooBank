using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models.Queries.Transactions;

namespace Asm.MooBank.Importers;

public interface IImporter
{
    Task<TransactionImportResult> Import(Account account, Stream contents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reprocess existing transactions.
    /// </summary>
    /// <param name="account">The account to process</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Reprocess(Account account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a query request to get transaction extra details.
    /// </summary>
    /// <param name="transactions">The transaction to enrich.</param>
    /// <returns>A query request.</returns>
    GetTransactionExtraDetails? CreateExtraDetailsRequest(Guid accountId, Models.PagedResult<Models.Transaction> transactions);
}
