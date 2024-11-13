using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Bills.Models;
public record Account : Instrument
{

}

public static class AccountExtensions
{
    public static Account ToModel(this Domain.Entities.Utility.Account account)
    {
        return new Account
        {
            Id = account.Id,
            Name = account.Name,
            Currency = account.Currency,
            Controller = account.Controller,
            CurrentBalance = 0,
            CurrentBalanceLocalCurrency = 0,
        };
    }
    public static IEnumerable<Account> ToModel(this IEnumerable<Domain.Entities.Utility.Account> accounts)
    {
        return accounts.Select(ToModel);
    }
}
