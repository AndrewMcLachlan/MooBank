using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.AccountGroup;

public record GetAll : IQuery<IEnumerable<Models.AccountGroup>>;

internal class GetAllHandler : IQueryHandler<GetAll, IEnumerable<Models.AccountGroup>>
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly IQueryable<Domain.Entities.AccountGroup.AccountGroup> _accountGroups;

    public GetAllHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, IUserDataProvider userDataProvider)
    {
        _userDataProvider = userDataProvider;
        _accountGroups = accountGroups;
    }

    public Task<IEnumerable<Models.AccountGroup>> Handle(GetAll _, CancellationToken cancellationToken) =>
        _accountGroups.Where(ag => ag.OwnerId == _userDataProvider.CurrentUserId).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
}
