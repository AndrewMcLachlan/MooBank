namespace Asm.MooBank.Domain.Entities.Account;

public partial class ImportAccount
{
    public Guid AccountId { get; set; }

    public int ImporterTypeId { get; set; }

    public virtual ImporterType ImporterType { get; set; }
}
