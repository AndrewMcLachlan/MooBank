using System.ComponentModel.DataAnnotations.Schema;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Domain.Entities.User;

[AggregateRoot]
public partial class User(Guid id) : KeyedEntity<Guid>(id)
{
    public User() : this(default) { }

    [Required]
    [MaxLength(255)]
    public required string EmailAddress { get; set; }

    [MaxLength(255)]
    public string? FirstName { get; set; }

    [MaxLength(255)]
    public string? LastName { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "AUD";

    [Required]
    public Guid FamilyId { get; set; }

    public Guid? PrimaryAccountId { get; set; }

    public ICollection<Group.Group> Groups { get; set; } = new HashSet<Group.Group>(); 

    [NotMapped]
    public IEnumerable<Instrument.Instrument> Instruments => InstrumentOwners?.Select(a => a.Instrument) ?? [];

    public virtual ICollection<InstrumentOwner> InstrumentOwners { get; set; } = new HashSet<InstrumentOwner>();

    public virtual ICollection<UserCard> Cards { get; set; } = new HashSet<UserCard>();

    [ForeignKey(nameof(FamilyId))]
    public Family.Family Family { get; set; } = null!;
}
