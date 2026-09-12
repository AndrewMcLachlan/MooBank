using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Bills.Models;

public record Account : Instrument
{
    public UtilityType UtilityType { get; internal set; }

    public required string AccountNumber { get; init; }

    public bool ShareWithFamily { get; internal set; }

    public DateOnly? FirstBill { get; internal set; }

    public DateOnly? LatestBill { get; internal set; }

    /// <summary>
    /// Days between the last two bills: how often this account is billed.
    /// </summary>
    /// <remarks>
    /// Null until there are two bills to measure between. Lets a caller offer periods that suit the
    /// account -- three and six month windows say nothing about an account billed quarterly --
    /// without fetching bills to work out the cadence for itself.
    /// </remarks>
    public int? BillingIntervalDays { get; internal set; }
}

public static class AccountExtensions
{
    public static Account ToModel(this Domain.Entities.Utility.Account account)
    {
        // The gap between the last two issue dates, which is the cadence as it stands now rather
        // than an average over an account that may have changed how often it bills.
        var recent = account.Bills.OrderByDescending(b => b.IssueDate).Take(2).ToList();

        return new Account
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description,
            Currency = account.Currency,
            Controller = account.Controller,
            CurrentBalance = 0,
            CurrentBalanceLocalCurrency = 0,
            UtilityType = account.UtilityType,
            AccountNumber = account.AccountNumber,
            ShareWithFamily = account.ShareWithFamily,
            FirstBill = account.Bills.Count != 0 ? account.Bills.Min(b => b.IssueDate) : null,
            LatestBill = account.Bills.Count != 0 ? account.Bills.Max(b => b.IssueDate) : null,
            BillingIntervalDays = recent.Count == 2 ? recent[0].IssueDate.DayNumber - recent[1].IssueDate.DayNumber : null,
        };
    }
    public static IEnumerable<Account> ToModel(this IEnumerable<Domain.Entities.Utility.Account> accounts)
    {
        return accounts.Select(ToModel);
    }
}
