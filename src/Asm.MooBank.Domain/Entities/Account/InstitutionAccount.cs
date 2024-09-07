using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public class InstitutionAccount(Guid id) : TransactionInstrument(id)
{
    public InstitutionAccount() : this(Guid.Empty) { }

    public int InstitutionId { get; set; }

    [Column(TypeName = "datetimeoffset(0)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public new DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.Now;

    public bool IncludeInBudget { get; set; }

    [Column("AccountTypeId")]
    public AccountType AccountType { get; set; }

    public virtual ImportAccount? ImportAccount { get; set; }

    [ForeignKey(nameof(InstitutionId))]
    public virtual Institution.Institution Institution { get; set; } = null!;

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

    public override Group.Group? GetGroup(Guid user) =>
        base.GetGroup(user) ??
        ValidViewers.Where(a => a.UserId == user).Select(aah => aah.Group).SingleOrDefault();
}
