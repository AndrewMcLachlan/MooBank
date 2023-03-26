using Asm.Cqrs.Queries;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.AccountGroup;

internal class GetAccountGroups : IQueryHandler<Models.Queries.AccountGroup.GetAccountGroups, IEnumerable<Models.AccountGroup>>
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly IQueryable<Domain.Entities.AccountGroup.AccountGroup> _accountGroups;

    public GetAccountGroups(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, IUserDataProvider userDataProvider)
    {
        _userDataProvider = userDataProvider;
        _accountGroups = accountGroups;
    }

    public Task<IEnumerable<Models.AccountGroup>> Handle(Models.Queries.AccountGroup.GetAccountGroups _, CancellationToken cancellationToken) =>
        _accountGroups.Where(ag => ag.OwnerId == _userDataProvider.CurrentUserId).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
}
