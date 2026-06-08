using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public class LogicalAccount(Guid id, IEnumerable<InstitutionAccount> institutionAccounts) : TransactionInstrument(id)
{
    private readonly List<InstitutionAccount> _institutionAccounts = [.. institutionAccounts];

    private readonly List<AccountTagPurpose> _tagPurposes = [];

    public LogicalAccount() : this(Guid.Empty, []) { }

    public bool IncludeInBudget { get; set; }

    [Column("AccountTypeId")]
    public AccountType AccountType { get; set; }

    public IReadOnlyCollection<InstitutionAccount> InstitutionAccounts { get => _institutionAccounts; internal init => _institutionAccounts = [.. value]; }

    public IReadOnlyCollection<AccountTagPurpose> TagPurposes { get => _tagPurposes; internal init => _tagPurposes = [.. value]; }

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
    }

    public void SetTagPurpose(TagPurpose purpose, int? tagId)
    {
        var existing = _tagPurposes.FirstOrDefault(t => t.Purpose == purpose);

        if (tagId is null)
        {
            if (existing is not null) _tagPurposes.Remove(existing);
            return;
        }

        if (existing is not null)
        {
            existing.TagId = tagId.Value;
        }
        else
        {
            _tagPurposes.Add(new AccountTagPurpose { InstrumentId = Id, Purpose = purpose, TagId = tagId.Value });
        }
    }
}
