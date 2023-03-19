using System.Collections.Generic;

namespace Asm.MooBank.Models;

public record AccountsList
{
    public IEnumerable<InstitutionAccount> PositionedAccounts { get; set; }

    public IEnumerable<InstitutionAccount> OtherAccounts { get; set; }

    public decimal Position { get; set; }

}