using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

[AggregateRoot]
public partial class ImporterType
{
    public int ImporterTypeId { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public Type AsType() => System.Type.GetType(Type) ?? throw new InvalidOperationException($"Invalid type name {Type}");

    //public virtual ICollection<ImportAccount> ImportAccounts { get; set; }
}
