using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asm.MooBank.Modules.Accounts.Models.InstitutionAccount;

public record CreateInstitutionAccount
{
    public int InstitutionId { get; init; }

    public int? ImporterTypeId { get; init; }

    public required string Name { get; init; }

    public DateOnly OpenedDate { get; init; }
}
