using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.AccountGroup;

internal class GetAccountGroupsHandler : IQueryHandler<GetAccountGroups, IEnumerable<Models.AccountGroup>>
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly IQueryable<Domain.Entities.AccountGroup.AccountGroup> _accountGroups;

    public GetAccountGroupsHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, IUserDataProvider userDataProvider)
    {
        _userDataProvider = userDataProvider;
        _accountGroups = accountGroups;
    }

    public Task<IEnumerable<Models.AccountGroup>> Handle(GetAccountGroups _, CancellationToken cancellationToken) =>
        _accountGroups.Where(ag => ag.OwnerId == _userDataProvider.CurrentUserId).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
}
