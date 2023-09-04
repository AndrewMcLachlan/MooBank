using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.AccountGroup;

public record GetAll : IQuery<IEnumerable<Models.AccountGroup>>;

internal class GetAllHandler : QueryHandlerBase, IQueryHandler<GetAll, IEnumerable<Models.AccountGroup>>
{
    private readonly IQueryable<Domain.Entities.AccountGroup.AccountGroup> _accountGroups;

    public GetAllHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, Models.AccountHolder accountHolder) : base(accountHolder)
    {
        _accountGroups = accountGroups;
    }

    public Task<IEnumerable<Models.AccountGroup>> Handle(GetAll _, CancellationToken cancellationToken) =>
        _accountGroups.Where(ag => ag.OwnerId == AccountHolder.Id).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
}
