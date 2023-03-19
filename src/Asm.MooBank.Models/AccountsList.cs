using System.Collections.Generic;

namespace Asm.MooBank.Models;

public record AccountsList
{
    public IEnumerable<Account> PositionedAccounts { get; set; }

    public IEnumerable<Account> OtherAccounts { get; set; }

    public decimal Position { get; set; }

}