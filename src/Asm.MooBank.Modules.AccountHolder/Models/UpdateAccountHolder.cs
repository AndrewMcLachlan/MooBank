namespace Asm.MooBank.Modules.Users.Models;
public record UpdateAccountHolder
{
    public Guid? PrimaryAccountId { get; set; }
    public string Currency { get; set; } = "AUD";
    public IEnumerable<AccountHolderCard> Cards { get; set; } = [];
}
