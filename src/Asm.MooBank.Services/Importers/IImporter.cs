using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models.Queries.Transactions;

namespace Asm.MooBank.Importers;

public interface IImporter
{
    Task<TransactionImportResult> Import(Account account, Stream contents, CancellationToken cancellationToken = default);

    GetTransactionExtraDetails? CreateExtraDetailsRequest(Models.PagedResult<Models.Transaction> transactions);
}
