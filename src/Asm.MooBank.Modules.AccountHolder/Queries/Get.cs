namespace Asm.MooBank.Modules.AccountHolder.Queries;

internal record Get() : IQuery<Models.AccountHolder>;

internal class GetHandler(IQueryable<Domain.Entities.AccountHolder.AccountHolder> accountHolders, MooBank.Models.AccountHolder accountHolder) : IQueryHandler<Get, Models.AccountHolder>
{
    public async ValueTask<Models.AccountHolder> Handle(Get query, CancellationToken cancellationToken)
    {
        var entity = await accountHolders.Include(a => a.Cards).SingleAsync(a => a.Id == accountHolder.Id, cancellationToken);

        return new Models.AccountHolder
        {
            Currency = entity.Currency,
            EmailAddress = entity.EmailAddress,
            FamilyId = entity.FamilyId,
            FirstName = entity.FirstName,
            Id = entity.Id,
            LastName = entity.LastName,
            PrimaryAccountId = entity.PrimaryAccountId,
            Cards = entity.Cards.Select(c => new Models.AccountHolderCard
            {
                Last4Digits = c.Last4Digits,
                Name = c.Name,
            }),
            Accounts = accountHolder.Accounts,
            SharedAccounts = accountHolder.SharedAccounts,
        };
    }
}
