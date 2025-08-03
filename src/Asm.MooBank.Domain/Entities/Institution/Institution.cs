using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Institution;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
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
