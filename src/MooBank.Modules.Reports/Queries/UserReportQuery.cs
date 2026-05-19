using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

/// <summary>
/// Base for report queries that span every instrument the current user has access to.
/// Date range defaults to the previous calendar month because imported data is usually a month behind.
/// </summary>
public abstract record UserReportQuery
{
    public DateOnly? Start { get; init; }

    public DateOnly? End { get; init; }

    public (DateOnly Start, DateOnly End) ResolveRange()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
        var lastMonthEnd = thisMonthStart.AddDays(-1);
        var lastMonthStart = new DateOnly(lastMonthEnd.Year, lastMonthEnd.Month, 1);

        return (Start ?? lastMonthStart, End ?? lastMonthEnd);
    }
}

internal static class UserReportScope
{
    /// <summary>
    /// Accounts a user typically thinks of as "transactional" — bank accounts they spend from and credit cards.
    /// </summary>
    public static readonly AccountType[] TransactionalTypes = [AccountType.Transaction, AccountType.Credit];

    public static IQueryable<LogicalAccount> AccessibleTo(this IQueryable<LogicalAccount> accounts, User user) =>
        accounts.Apply(new OpenAccessibleSpecification<LogicalAccount>(user.Id, user.FamilyId));

    public static IQueryable<LogicalAccount> Transactional(this IQueryable<LogicalAccount> accounts) =>
        accounts.Where(a => TransactionalTypes.Contains(a.AccountType));
}
