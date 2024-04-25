namespace Asm.MooBank.Domain.Entities.Account;

public partial class AccountAccountHolder
{
    public Guid AccountId { get; set; }
    public Guid AccountHolderId { get; set; }

    public Guid? AccountGroupId { get; set; }

    public virtual Instrument Account { get; set; } = null!;

    public virtual AccountHolder.AccountHolder AccountHolder { get; set; } = null!;

    public virtual Group.Group? AccountGroup { get; set; }
}
