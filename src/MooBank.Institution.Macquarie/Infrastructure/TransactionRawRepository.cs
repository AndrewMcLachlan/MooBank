using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.Macquarie.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Institution.Macquarie.Infrastructure;

internal class TransactionRawRepository(MooBankContext context) : Asm.Domain.Infrastructure.RepositoryWriteBase<MooBankContext, TransactionRaw, Guid>(context), ITransactionRawRepository
{
    public async Task<IEnumerable<TransactionRaw>> GetAll(Guid accountId, CancellationToken cancellationToken = default) =>
        await Entities.Include(t => t.Transaction).Where(t => t.AccountId == accountId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TransactionRaw>> GetUnprocessed(IEnumerable<Guid> transactionIds, CancellationToken cancellationToken = default) =>
        await Entities.Where(t => !transactionIds.Contains(t.Id)).ToArrayAsync(cancellationToken);

    public Task<TransactionRaw> GetZeroBalance(string details, DateOnly transactionTime, decimal debit, decimal credit, CancellationToken cancellationToken = default) =>
        Entities.FirstAsync(t => t.Details == details && t.Date == transactionTime && t.Debit == debit && t.Credit == credit && t.Balance == 0, cancellationToken);
}
