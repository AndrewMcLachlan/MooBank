using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

[AggregateRoot]
public partial class ImporterType
{
    [Key]
    public int ImporterTypeId { get; set; }

    [Required]
    public required string Type { get; set; }

    [Required]
    public required string Name { get; set; }

    public Type AsType() => System.Type.GetType(Type) ?? throw new InvalidOperationException($"Invalid type name {Type}");
}
