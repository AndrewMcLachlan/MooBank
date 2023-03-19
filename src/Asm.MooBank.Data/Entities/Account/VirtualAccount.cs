namespace Asm.MooBank.Domain.Entities.Account;

public partial class VirtualAccount : Account
{
    public VirtualAccount()
    {
    }

    public Guid InstitutionAccountId { get; set; }

    public virtual InstitutionAccount InstitutionAccount { get; set; }
}
