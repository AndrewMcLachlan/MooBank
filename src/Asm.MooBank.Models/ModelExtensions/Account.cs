namespace Asm.MooBank.Models;

public partial record Account
{


    public static implicit operator Account(Domain.Entities.Account.Account account)
    {
        return new Account
        {
            Id = account.AccountId,
            Name = account.Name,
            Description = account.Description,
            CurrentBalance = account.Balance,
            BalanceDate = account.LastUpdated,
            AccountType = GetAccountType(account),
        };
    }

    public static explicit operator Domain.Entities.Account.Account(Account account)
    {
        return new Domain.Entities.Account.Account
        {
            AccountId = account.Id == Guid.Empty ? Guid.NewGuid() : account.Id,
            Name = account.Name,
            Description = account.Description,
            Balance = account.CurrentBalance,
            LastUpdated = account.BalanceDate,
        };
    }

    private static string? GetAccountType(Domain.Entities.Account.Account account) =>
        account switch
        {
            Domain.Entities.Account.InstitutionAccount iAccount => iAccount.AccountType.ToString(),
            _ => null,
        };
}
