using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.Institution;

[AggregateRoot]
public class Institution : KeyedEntity<int>
{
    public Institution() : base(default)
    {

    }

    public Institution([DisallowNull] int id) : base(id)
    {
    }

    public string Name { get; set; } = null!;

    [Column("InstitutionTypeId")]
    public virtual InstitutionType InstitutionType { get; set; }
}
