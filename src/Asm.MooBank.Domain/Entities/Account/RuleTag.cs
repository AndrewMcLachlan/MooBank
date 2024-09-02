using Asm.MooBank.Domain.Entities.Instrument;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account;

[PrimaryKey(nameof(RuleId), nameof(TagId))]
public class RuleTag
{
    public int RuleId { get; set; }

    public int TagId { get; set; }

    public Rule Rule { get; set; } = null!;

    public Tag.Tag Tag { get; set; } = null!;
}
