namespace Asm.MooBank.Domain.Entities.ReferenceData;

public partial class ImporterType
{
    public int ImporterTypeId { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }

    public Type AsType() => System.Type.GetType(Type) ?? throw new InvalidOperationException($"Invalid type name {Type}");

    //public virtual ICollection<ImportAccount> ImportAccounts { get; set; }
}
