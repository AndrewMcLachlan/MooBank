namespace Asm.MooBank.Domain.Entities.Account;

public partial class AccountAccountHolder
{
    public Guid AccountId { get; set; }
    public Guid AccountHolderId { get; set; }

    public virtual Account Account { get; set; }

    public virtual AccountHolder.AccountHolder AccountHolder { get; set; }
}
