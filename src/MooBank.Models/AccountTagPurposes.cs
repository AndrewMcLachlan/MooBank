namespace Asm.MooBank.Models;

public static class AccountTagPurposes
{
    private static readonly IReadOnlyList<TagPurpose> Empty = [];

    private static readonly IReadOnlyList<TagPurpose> Savings = [TagPurpose.Interest];

    public static IReadOnlyList<TagPurpose> For(AccountType accountType) => accountType switch
    {
        AccountType.Savings => Savings,
        _ => Empty,
    };
}
