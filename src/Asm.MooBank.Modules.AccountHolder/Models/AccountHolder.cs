namespace Asm.MooBank.Modules.AccountHolder.Models;

public record AccountHolder : MooBank.Models.User
{
    public IEnumerable<AccountHolderCard> Cards { get; set; } = [];
}

public static class AccountHolderExtensions
{
    public static Models.AccountHolder ToModel(this Domain.Entities.AccountHolder.User accountHolder)
    {
        return new Models.AccountHolder
        {
            Id = accountHolder.Id,
            EmailAddress = accountHolder.EmailAddress,
            FamilyId = accountHolder.FamilyId,
            FirstName = accountHolder.FirstName,
            LastName = accountHolder.LastName,
            Currency = accountHolder.Currency,
            PrimaryAccountId = accountHolder.PrimaryAccountId,
            Cards = accountHolder.Cards.Select(c => new Models.AccountHolderCard
            {
                Name = c.Name,
                Last4Digits = c.Last4Digits,
            }).ToList(),
        };
    }
}
