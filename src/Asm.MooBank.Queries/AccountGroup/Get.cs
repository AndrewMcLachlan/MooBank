namespace Asm.MooBank.Queries.AccountGroup;

public record Get(Guid Id) : IQuery<Models.AccountGroup>;

internal class GetHandler : QueryHandlerBase, IQueryHandler<Get, Models.AccountGroup>
{
    private readonly IQueryable<Domain.Entities.AccountGroup.AccountGroup> _accountGroups;

    public GetHandler(IQueryable<Domain.Entities.AccountGroup.AccountGroup> accountGroups, Models.AccountHolder accountHolder) : base(accountHolder)
    {
        _accountGroups = accountGroups;
    }

    public async Task<Models.AccountGroup> Handle(Get request, CancellationToken cancellationToken)
    {
        var accountGroup = await _accountGroups.SingleOrDefaultAsync(a => a.Id == request.Id && a.OwnerId == AccountHolder.Id, cancellationToken);

        if (accountGroup == null) throw new NotFoundException($"Account Group with ID {request.Id} could not be found");

        return (Models.AccountGroup)accountGroup;
    }
}
