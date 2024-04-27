using Asm.MooBank.Models;
using Asm.MooBank.Modules.Asset.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Asset.Queries;

public sealed record Get(Guid Id) : IQuery<Models.Asset>;

internal class GetHandler(IQueryable<Domain.Entities.Asset.Asset> accounts, User accountHolder, ISecurity security) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Models.Asset>
{
    public async ValueTask<Models.Asset> Handle(Get query, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.Owners).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Owners).ThenInclude(ah => ah.User)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.User)
                                   .SingleOrDefaultAsync(a => a.Id == query.Id, cancellationToken) ?? throw new NotFoundException();
        security.AssertAccountPermission(entity);
        var account = entity.ToModel(AccountHolder.Id);

        return account!;
    }
}
