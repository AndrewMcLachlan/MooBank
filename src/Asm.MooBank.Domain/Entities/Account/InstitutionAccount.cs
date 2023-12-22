using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public class InstitutionAccount(Guid id) : TransactionAccount(id)
{
    public int InstitutionId { get; set; }

    public bool IncludeInPosition { get; set; }

    public new DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.Now;

    public bool IncludeInBudget { get; set; }

    [Column("AccountControllerId")]
    public AccountController AccountController { get; set; }

    [Column("AccountTypeId")]
    public AccountType AccountType { get; set; }

    public virtual ImportAccount? ImportAccount { get; set; }

    public virtual ICollection<VirtualAccount> VirtualAccounts { get; set; } = new HashSet<VirtualAccount>();

    public virtual Institution.Institution Institution { get; set; } = null!;

    [NotMapped]
    public IEnumerable<AccountAccountViewer> ValidAccountViewers
    {
        get
        {
            if (!ShareWithFamily) return [];
            var familyIds = AccountHolders.Select(a => a.AccountHolder.FamilyId).Distinct();
            return AccountViewers.Where(a => familyIds.Contains(a.AccountHolder.FamilyId));
        }
    }

    public override AccountGroup.AccountGroup? GetAccountGroup(Guid accountHolderId) =>
        base.GetAccountGroup(accountHolderId) ??
        ValidAccountViewers.Where(a => a.AccountHolderId == accountHolderId).Select(aah => aah.AccountGroup).SingleOrDefault();
}
