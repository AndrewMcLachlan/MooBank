using Asm.MooBank.Modules.Account.Models.Recurring;

namespace Asm.MooBank.Modules.Account.Models.Account;

public record VirtualAccount : TransactionAccount
{
    public Guid ParentId { get; set; }

    public IEnumerable<RecurringTransaction> RecurringTransactions { get; set; } = [];
}

public static class VirtualAccountExtensions
{
    public static VirtualAccount ToModel(this Domain.Entities.Account.VirtualAccount account)
    {
        return new VirtualAccount
        {
            Id = account.Id,
            ParentId = account.ParentAccountId,
            Name = account.Name,
            Description = account.Description,
            CurrentBalance = account.Balance,
            CalculatedBalance = account.CalculatedBalance,
            BalanceDate = account.LastUpdated,
            LastTransaction = account.LastTransaction,
            RecurringTransactions = account.RecurringTransactions.ToModel(),
        };
    }

    public static IEnumerable<VirtualAccount> ToModel(this IEnumerable<Domain.Entities.Account.VirtualAccount> accounts) =>
        accounts.Select(a => a.ToModel());

    public static Domain.Entities.Account.VirtualAccount ToEntity(this VirtualAccount account, Guid parentAccountId) => new(account.Id)
    {
        ParentAccountId = parentAccountId,
        Name = account.Name,
        Description = account.Description,
        Balance = account.CurrentBalance,
    };
}
