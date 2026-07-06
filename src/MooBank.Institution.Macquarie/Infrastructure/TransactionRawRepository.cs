using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.Macquarie.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Institution.Macquarie.Infrastructure;

internal class TransactionRawRepository(MooBankContext context) : Asm.Domain.Infrastructure.RepositoryWriteBase<MooBankContext, TransactionRaw, Guid>(context), ITransactionRawRepository
{
    public async Task<IEnumerable<TransactionRaw>> GetAll(Guid accountId, CancellationToken cancellationToken = default) =>
        await Entities.Include(t => t.Transaction).Where(t => t.AccountId == accountId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TransactionRawSummary>> GetSummaries(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await Entities.AsNoTracking()
                      .Where(t => t.AccountId == accountId && t.Date >= startDate && t.Date <= endDate)
                      .Select(t => new TransactionRawSummary(t.Details, t.Date, t.Credit, t.Debit, t.Balance))
                      .ToListAsync(cancellationToken);

    public Task<TransactionRaw?> GetZeroBalance(Guid accountId, string details, DateOnly transactionTime, decimal debit, decimal credit, CancellationToken cancellationToken = default) =>
        Entities.FirstOrDefaultAsync(t => t.AccountId == accountId && t.Details == details && t.Date == transactionTime && t.Debit == debit && t.Credit == credit && t.Balance == 0, cancellationToken);
}
