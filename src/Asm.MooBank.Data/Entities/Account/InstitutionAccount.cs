using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public class InstitutionAccount : Account
{
    public InstitutionAccount()
    {
        VirtualAccounts = new HashSet<VirtualAccount>();
    }

    public int InstitutionId { get; set; }

    public bool IncludeInPosition { get; set; }

    public new DateTimeOffset LastUpdated { get; set; }

    public bool IncludeInBudget { get; set; }

    public bool ShareWithFamily { get; set; }

    [Column("AccountControllerId")]
    public AccountController AccountController { get; set; }

    [Column("AccountTypeId")]
    public AccountType AccountType { get; set; }

    public virtual ImportAccount? ImportAccount { get; set; }

    public virtual ICollection<VirtualAccount> VirtualAccounts { get; set; }

    public virtual Institution.Institution Institution { get; set; }
}
