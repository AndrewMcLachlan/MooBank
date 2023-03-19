namespace Asm.MooBank.Domain.Entities;

public partial class ImporterType
{
    public int ImporterTypeId { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }

    //public virtual ICollection<ImportAccount> ImportAccounts { get; set; }
}
