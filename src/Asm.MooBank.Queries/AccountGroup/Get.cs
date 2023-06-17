namespace Asm.MooBank.Queries.AccountGroup;

public record Get(Guid Id) : IQuery<Models.AccountGroup>;

internal class GetHandler : IQueryHandler<Get, Models.AccountGroup>
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly IQueryable<Domain.Entities.AccountGroup.AccountGroup> _accountGroups;

    public GetHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, IUserDataProvider userDataProvider)
    {
        _accountGroups = accountGroups;
        _userDataProvider = userDataProvider;
    }

    public async Task<Models.AccountGroup> Handle(Get request, CancellationToken cancellationToken)
    {
        var accountGroup = await _accountGroups.SingleOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == _userDataProvider.CurrentUserId, cancellationToken);

        if (accountGroup == null) throw new NotFoundException($"Account Group with ID {request.Id} could not be found");

        return (Models.AccountGroup)accountGroup;
    }
}
