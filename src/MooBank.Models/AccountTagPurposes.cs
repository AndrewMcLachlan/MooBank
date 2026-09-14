namespace Asm.MooBank.Models;

public static class AccountTagPurposes
{
    private static readonly IReadOnlyList<TagPurpose> _empty = [];

    private static readonly IReadOnlyList<TagPurpose> _savings = [TagPurpose.Interest];

    private static readonly IReadOnlyList<TagPurpose> _superannuation =
    [
        TagPurpose.EmployerContribution,
        TagPurpose.PersonalContribution,
    ];

    private static readonly IReadOnlyList<TagPurpose> _mortgageOrLoan = [TagPurpose.MortgageInterest];

    public static IReadOnlyList<TagPurpose> For(AccountType accountType) => accountType switch
    {
        AccountType.Savings => _savings,
        AccountType.Superannuation => _superannuation,
        AccountType.Mortgage => _mortgageOrLoan,
        AccountType.Loan => _mortgageOrLoan,
        _ => _empty,
    };
}
