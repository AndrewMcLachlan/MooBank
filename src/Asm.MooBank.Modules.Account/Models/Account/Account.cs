namespace Asm.MooBank.Modules.Account.Models.Account;

public partial record Account
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset BalanceDate { get; set; } = DateTimeOffset.Now;

    public DateOnly? LastTransaction { get; set; }

    public required decimal CurrentBalance { get; set; }

    public decimal CalculatedBalance { get; set; }

    public string? AccountType { get; set; }

    public Guid? AccountGroupId { get; set; }
}

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
            CalculatedBalance = account.CalculatedBalance,
            BalanceDate = account.LastUpdated,
            LastTransaction = account.LastTransaction,
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
            //AccountGroups = account.AccountGroupId == null ? Array.Empty<Domain.Entities.AccountGroup.AccountGroup>() : new Domain.Entities.AccountGroup.AccountGroup[] { new Domain.Entities.AccountGroup.AccountGroup { Id = account.AccountGroupId.Value } }
        };
    }

    private static string? GetAccountType(Domain.Entities.Account.Account account) =>
        account switch
        {
            Domain.Entities.Account.InstitutionAccount iAccount => iAccount.AccountType.ToString(),
            _ => null,
        };
}