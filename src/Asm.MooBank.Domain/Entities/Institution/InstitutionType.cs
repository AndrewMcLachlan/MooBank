using System.Diagnostics.CodeAnalysis;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Institution;

[AggregateRoot]
public class InstitutionType : KeyedEntity<int>
{
    public InstitutionType() : base(default)
    {

    }

    public InstitutionType([DisallowNull] int id) : base(id)
    {
    }

    public string Name { get; set; } = null!;

}
