using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Modules.Users.Queries;

internal record Get() : IQuery<Models.User>;

internal class GetHandler(IQueryable<User> accountHolders, MooBank.Models.User user) : IQueryHandler<Get, Models.User>
{
    public async ValueTask<Models.User> Handle(Get query, CancellationToken cancellationToken)
    {
        var entity = await accountHolders.Include(a => a.Cards).SingleAsync(a => a.Id == user.Id, cancellationToken);

        return new Models.User
        {
            Currency = entity.Currency,
            EmailAddress = entity.EmailAddress,
            FamilyId = entity.FamilyId,
            FirstName = entity.FirstName,
            Id = entity.Id,
            LastName = entity.LastName,
            PrimaryAccountId = entity.PrimaryAccountId,
            Cards = entity.Cards.Select(c => new Models.UserCard
            {
                Last4Digits = c.Last4Digits,
                Name = c.Name,
            }),
            Accounts = user.Accounts,
            SharedAccounts = user.SharedAccounts,
        };
    }
}
