using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public class LogicalAccount : TransactionInstrument
{
    private readonly List<InstitutionAccount> _institutionAccounts;

    private readonly List<AccountTagPurpose> _tagPurposes = [];

    internal LogicalAccount(Guid id, IEnumerable<InstitutionAccount> institutionAccounts) : base(id)
    {
        _institutionAccounts = [.. institutionAccounts];
    }

    // For EF materialisation only. Construct through Create.
    internal LogicalAccount() : this(Guid.Empty, []) { }

    public static LogicalAccount Create(string name, string? description, string currency, AccountType accountType, Controller controller, bool includeInBudget, bool shareWithFamily, InstitutionAccount institutionAccount, decimal openingBalance, DateOnly openedDate)
    {
        var account = new LogicalAccount
        {
            Name = name,
            Description = description,
            Currency = currency,
            AccountType = accountType,
            Controller = controller,
            IncludeInBudget = includeInBudget,
            ShareWithFamily = shareWithFamily,
        };

        account.AddInstitutionAccount(institutionAccount);
        account.MarkCreated();
        account.Events.Add(new AccountAddedEvent(account, openingBalance, openedDate));

        return account;
    }

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

    public void Update(string name, string? description, Controller controller, AccountType accountType, bool shareWithFamily, bool includeInBudget)
    {
        Name = name;
        Description = description;
        Controller = controller;
        AccountType = accountType;
        ShareWithFamily = shareWithFamily;
        IncludeInBudget = includeInBudget;

        MarkUpdated();
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
            _tagPurposes.Add(new AccountTagPurpose
            {
                InstrumentId = Id,
                Purpose = purpose,
                TagId = tagId.Value,
                LogicalAccount = this,
            });
        }
    }
}
