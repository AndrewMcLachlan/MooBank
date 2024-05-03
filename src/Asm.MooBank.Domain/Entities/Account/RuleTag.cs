using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Domain.Entities.Account;

public class RuleTag
{
    public int RuleId { get; set; }

    public int TagId { get; set; }

    public Rule Rule { get; set; } = null!;

    public Tag.Tag Tag { get; set; } = null!;
}
