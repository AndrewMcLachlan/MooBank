using System.Collections.Generic;

namespace Asm.MooBank.Models;

public class AccountsList
{
    public IEnumerable<Account> PositionedAccounts { get; set; }

    public IEnumerable<Account> OtherAccounts { get; set; }

    public decimal Position { get; set; }

}