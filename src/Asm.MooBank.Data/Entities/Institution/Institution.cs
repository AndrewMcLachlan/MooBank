using System.Diagnostics.CodeAnalysis;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Institution;
public class Institution : KeyedEntity<int>
{
    public Institution() : base(default)
    {

    }

    public Institution([DisallowNull] int id) : base(id)
    {
    }

    public string Name { get; set; } = null!;

    public int InstitutionTypeId { get; set; }

    public virtual InstitutionType InstitutionType { get; set; } = null!;
}
