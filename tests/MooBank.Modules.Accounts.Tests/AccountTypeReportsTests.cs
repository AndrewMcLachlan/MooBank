#nullable enable
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Accounts.Tests;

/// <summary>
/// Unit tests for <see cref="AccountTypeReports"/>.
/// </summary>
public class AccountTypeReportsTests
{
    /// <summary>
    /// Given a known account type
    /// When the report matrix is requested
    /// Then the expected ordered list of report kinds is returned.
    /// </summary>
    [Theory]
    [InlineData(AccountType.Transaction, new[] { ReportKind.InOut, ReportKind.TopTags, ReportKind.Breakdown, ReportKind.TagTrend, ReportKind.AllTags, ReportKind.MonthlyBalances })]
    [InlineData(AccountType.Savings, new[] { ReportKind.MonthlyBalances, ReportKind.SavingsInterest, ReportKind.TagTrend, ReportKind.AllTags, ReportKind.InOut })]
    [InlineData(AccountType.Superannuation, new[] { ReportKind.MonthlyBalances, ReportKind.SuperContributions, ReportKind.SuperReturns, ReportKind.TagTrend, ReportKind.AllTags })]
    [InlineData(AccountType.Mortgage, new[] { ReportKind.MonthlyBalances, ReportKind.TagTrend, ReportKind.AllTags })]
    [Trait("Category", "Unit")]
    public void For_KnownAccountType_ReturnsExpectedReports(AccountType accountType, ReportKind[] expected)
    {
        // Act
        var result = AccountTypeReports.For(accountType);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Given an account type of NotSet (default / legacy data)
    /// When the report matrix is requested
    /// Then the Transaction-account defaults are returned as a safe fallback.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void For_NotSet_ReturnsTransactionDefaults()
    {
        // Act
        var result = AccountTypeReports.For(AccountType.NotSet);

        // Assert
        Assert.Equal(AccountTypeReports.For(AccountType.Transaction), result);
    }

    /// <summary>
    /// Given any account type
    /// When the report matrix is requested
    /// Then the returned list is never empty (every type gets at least one report).
    /// </summary>
    [Theory]
    [InlineData(AccountType.NotSet)]
    [InlineData(AccountType.Transaction)]
    [InlineData(AccountType.Savings)]
    [InlineData(AccountType.Credit)]
    [InlineData(AccountType.Mortgage)]
    [InlineData(AccountType.Superannuation)]
    [InlineData(AccountType.Investment)]
    [InlineData(AccountType.Loan)]
    [InlineData(AccountType.Broker)]
    [Trait("Category", "Unit")]
    public void For_AnyAccountType_ReturnsAtLeastOneReport(AccountType accountType)
    {
        // Act
        var result = AccountTypeReports.For(accountType);

        // Assert
        Assert.NotEmpty(result);
    }
}
