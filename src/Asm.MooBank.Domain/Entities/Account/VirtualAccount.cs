namespace Asm.MooBank.Domain.Entities.Account;

public partial class VirtualAccount(Guid id) : TransactionAccount(id)
{
    public Guid InstitutionAccountId { get; set; }

    public virtual InstitutionAccount InstitutionAccount { get; set; } = null!;
}
