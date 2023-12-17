namespace Asm.MooBank.Domain.Entities.Account;

public partial class AccountAccountViewer
{
    public Guid AccountId { get; set; }
    public Guid AccountHolderId { get; set; }

    public Guid? AccountGroupId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual AccountHolder.AccountHolder AccountHolder { get; set; } = null!;

    public virtual AccountGroup.AccountGroup AccountGroup { get; set; } = null!;
}
