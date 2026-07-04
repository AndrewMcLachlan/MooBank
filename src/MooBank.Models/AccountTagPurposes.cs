namespace Asm.MooBank.Models;

public static class AccountTagPurposes
{
    private static readonly IReadOnlyList<TagPurpose> Empty = [];

    private static readonly IReadOnlyList<TagPurpose> Savings = [TagPurpose.Interest];

    private static readonly IReadOnlyList<TagPurpose> Superannuation =
    [
        TagPurpose.EmployerContribution,
        TagPurpose.PersonalContribution,
    ];

    private static readonly IReadOnlyList<TagPurpose> MortgageOrLoan = [TagPurpose.MortgageInterest];

    public static IReadOnlyList<TagPurpose> For(AccountType accountType) => accountType switch
    {
        AccountType.Savings => Savings,
        AccountType.Superannuation => Superannuation,
        AccountType.Mortgage => MortgageOrLoan,
        AccountType.Loan => MortgageOrLoan,
        _ => Empty,
    };
}
