namespace Asm.MooBank.Web.Models;

public class AccountsModel
{
    public IEnumerable<MooBank.Models.Account> PositionedAccounts { get; set; }

    public IEnumerable<MooBank.Models.Account> OtherAccounts { get; set; }

    public decimal Position { get; set; }

}