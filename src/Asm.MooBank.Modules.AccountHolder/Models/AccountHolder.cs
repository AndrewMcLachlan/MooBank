namespace Asm.MooBank.Modules.Users.Models;

public record AccountHolder : MooBank.Models.User
{
    public IEnumerable<AccountHolderCard> Cards { get; set; } = [];
}

public static class AccountHolderExtensions
{
    public static AccountHolder ToModel(this Domain.Entities.AccountHolder.User user)
    {
        return new AccountHolder
        {
            Id = user.Id,
            EmailAddress = user.EmailAddress,
            FamilyId = user.FamilyId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Currency = user.Currency,
            PrimaryAccountId = user.PrimaryAccountId,
            Cards = user.Cards.Select(c => new AccountHolderCard
            {
                Name = c.Name,
                Last4Digits = c.Last4Digits,
            }).ToList(),
        };
    }
}
