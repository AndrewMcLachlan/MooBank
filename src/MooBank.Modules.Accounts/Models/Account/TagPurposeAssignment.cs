using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Accounts.Models.Account;

public record TagPurposeAssignment
{
    public required TagPurpose Purpose { get; init; }

    public required int TagId { get; init; }
}
