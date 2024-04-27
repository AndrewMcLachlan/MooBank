﻿using Asm.MooBank.Models;
using Asm.MooBank.Modules.Assets.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Assets.Queries;

public sealed record Get(Guid Id) : IQuery<Asset>;

internal class GetHandler(IQueryable<Domain.Entities.Asset.Asset> accounts, User user, ISecurity security) : IQueryHandler<Get, Asset>
{
    public async ValueTask<Asset> Handle(Get query, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.Owners).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Owners).ThenInclude(ah => ah.User)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.User)
                                   .SingleOrDefaultAsync(a => a.Id == query.Id, cancellationToken) ?? throw new NotFoundException();
        security.AssertAccountPermission(entity);
        var account = entity.ToModel(user.Id);

        return account!;
    }
}
