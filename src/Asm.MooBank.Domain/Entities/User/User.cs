using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities.User;

[AggregateRoot]
public partial class User(Guid id) : KeyedEntity<Guid>(id)
{
    public User() : this(default) { }

    public required string EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string Currency { get; set; } = "AUD";
    public required Guid FamilyId { get; set; }

    public Guid? PrimaryAccountId { get; set; }

    [NotMapped]
    public IEnumerable<Account.Instrument> Instruments => InstrumentOwners?.Select(a => a.Instrument) ?? [];

    public virtual ICollection<Account.InstrumentOwner> InstrumentOwners { get; set; } = new HashSet<Account.InstrumentOwner>();

    public virtual ICollection<AccountHolderCard> Cards { get; set; } = new HashSet<AccountHolderCard>();

    public Family.Family Family { get; set; } = null!;
}
