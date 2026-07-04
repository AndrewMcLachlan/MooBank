using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Domain.Entities.ReferenceData;
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

    [Required]
    public required string Name { get; set; }

    [Column("InstitutionTypeId")]
    public virtual InstitutionType InstitutionType { get; set; }

    public int? ImporterTypeId { get; set; }

    [ForeignKey(nameof(ImporterTypeId))]
    [AllowNull]
    public virtual ImporterType ImporterType { get; set; }
}
