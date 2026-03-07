namespace Asm.MooBank.Modules.Users.Models;

public record UpdateUser
{
    public Guid? PrimaryAccountId { get; set; }
    public string Currency { get; set; } = "AUD";
    public IEnumerable<UserCard> Cards { get; set; } = [];
}
