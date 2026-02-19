using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public class LogicalAccount(Guid id, IEnumerable<InstitutionAccount> institutionAccounts) : TransactionInstrument(id)
{
    private readonly List<InstitutionAccount> _institutionAccounts = [.. institutionAccounts];

    public LogicalAccount() : this(Guid.Empty, []) { }

    public bool IncludeInBudget { get; set; }

    [Column("AccountTypeId")]
    public AccountType AccountType { get; set; }

    public IReadOnlyCollection<InstitutionAccount> InstitutionAccounts { get => _institutionAccounts; internal init => _institutionAccounts = [.. value]; }

    [NotMapped]
    public IEnumerable<InstrumentViewer> ValidViewers
    {
        get
        {
            if (!ShareWithFamily) return [];
            var familyIds = Owners.Select(a => a.User.FamilyId).Distinct();
            return Viewers.Where(a => familyIds.Contains(a.User.FamilyId));
        }
    }

    public void AddInstitutionAccount(InstitutionAccount institutionAccount)
    {
        _institutionAccounts.Add(institutionAccount);
    }

    public override Group.Group? GetGroup(Guid user) =>
        base.GetGroup(user) ??
        ValidViewers.Where(a => a.UserId == user).Select(aah => aah.Group).SingleOrDefault();

    public void SetController(Controller controller)
    {
        Controller = controller;
        if (controller != Controller.Import)
        {
            foreach (var ia in InstitutionAccounts.Where(ia => ia.ImporterTypeId != null))
            {
                ia.ImporterTypeId = null;
            }
        }
    }
}
