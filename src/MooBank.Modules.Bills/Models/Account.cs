using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Bills.Models;

public record Account : Instrument
{
    public UtilityType UtilityType { get; internal set; }

    public DateOnly? FirstBill { get; internal set; }

    public DateOnly? LatestBill { get; internal set; }
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
            UtilityType = account.UtilityType,
            FirstBill = account.Bills.Count != 0 ? account.Bills.Min(b => b.IssueDate) : null,
            LatestBill = account.Bills.Count != 0 ? account.Bills.Max(b => b.IssueDate) : null,
        };
    }
    public static IEnumerable<Account> ToModel(this IEnumerable<Domain.Entities.Utility.Account> accounts)
    {
        return accounts.Select(ToModel);
    }
}
