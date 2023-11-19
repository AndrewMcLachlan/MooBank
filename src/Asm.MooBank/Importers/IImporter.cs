using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;

namespace Asm.MooBank.Importers;

public interface IImporter
{
    Task<TransactionImportResult> Import(Guid accountId, Stream contents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reprocess existing transactions.
    /// </summary>
    /// <param name="account">The account to process</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Reprocess(Guid accountId, CancellationToken cancellationToken = default);
}
