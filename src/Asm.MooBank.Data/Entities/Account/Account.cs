namespace Asm.MooBank.Domain.Entities.Account;

public class Account
{
    public Account()
    {
        Transactions = new HashSet<Transaction>();
        AccountHolders = new HashSet<AccountHolder.AccountHolder>();
    }

    public Guid AccountId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public decimal Balance { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }

    public virtual ICollection<AccountHolder.AccountHolder> AccountHolders { get; set; }
}
