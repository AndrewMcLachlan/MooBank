using Asm.MooBank.Domain.Entities.Account;

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
}
