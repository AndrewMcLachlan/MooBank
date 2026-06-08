using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account;

[PrimaryKey(nameof(InstrumentId), nameof(Purpose))]
public class AccountTagPurpose
{
    public Guid InstrumentId { get; set; }

    public TagPurpose Purpose { get; set; }

    public int TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    [AllowNull]
    public virtual Tag.Tag Tag { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    [AllowNull]
    public virtual LogicalAccount LogicalAccount { get; set; }
}
