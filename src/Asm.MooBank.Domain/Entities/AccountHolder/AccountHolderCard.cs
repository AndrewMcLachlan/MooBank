namespace Asm.MooBank.Domain.Entities.AccountHolder;
public class AccountHolderCard
{
    public Guid AccountHolderId { get; set; }

    public short Last4Digits { get; set; }

    public virtual AccountHolder AccountHolder { get; set; }
}
