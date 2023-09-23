using Asm.MooBank.Infrastructure;
using Asm.MooBank.Institution.Ing.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Institution.Ing.Infrastructure;
internal class TransactionRawRepository : Asm.Domain.Infrastructure.RepositoryBase<MooBankContext, TransactionRaw, Guid>, ITransactionRawRepository
{
    public TransactionRawRepository(MooBankContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionRaw>> GetAll(Guid accountId, CancellationToken cancellationToken = default) =>
        await Entities.Include(t => t.Transaction).Where(t => t.AccountId == accountId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TransactionRaw>> GetUnprocessed(IEnumerable<Guid> transactionIds, CancellationToken cancellationToken = default) =>
        await Entities.Where(t => !transactionIds.Contains(t.Id)).ToArrayAsync(cancellationToken);


}