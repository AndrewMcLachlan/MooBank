using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument;

[PrimaryKey(nameof(Id))]
public partial class Rule : KeyedEntity<int>
{
    public Rule() : base(default)
    {
    }

    public Rule([DisallowNull] int id) : base(id)
    {
    }

    public string? Description { get; set; }

    public Guid InstrumentId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Contains { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    [Navigation]
    public virtual partial Instrument Instrument { get; set; }

    public virtual ICollection<Tag.Tag> Tags { get; set; } = new HashSet<Tag.Tag>();
}
