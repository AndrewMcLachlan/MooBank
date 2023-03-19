using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Importers;

public interface IImporter
{
    Task<TransactionImportResult> Import(Account account, Stream contents, CancellationToken cancellationToken = default);
}
