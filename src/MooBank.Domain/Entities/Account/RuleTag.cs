using Asm.MooBank.Domain.Entities.Instrument;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account;

[PrimaryKey(nameof(RuleId), nameof(TagId))]
public partial class RuleTag
{
    public int RuleId { get; set; }

    public int TagId { get; set; }

    [Navigation]
    public partial Rule Rule { get; set; }

    [Navigation]
    public partial Tag.Tag Tag { get; set; }
}
