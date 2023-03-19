using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities.Account;

public class InstitutionAccount : Account
{
    public InstitutionAccount()
    {
        VirtualAccounts = new HashSet<VirtualAccount>();
        Transactions = new HashSet<Transaction>();
        AccountHolders = new HashSet<AccountHolder.AccountHolder>();
    }

    public bool IncludeInPosition { get; set; }

    public new DateTimeOffset LastUpdated { get; set; }

    [Column("AccountControllerId")]
    public AccountController AccountController { get; set; }

    [Column("AccountTypeId")]
    public AccountType AccountType { get; set; }

    public virtual ImportAccount? ImportAccount { get; set; }

    public virtual ICollection<VirtualAccount> VirtualAccounts { get; set; }
}
