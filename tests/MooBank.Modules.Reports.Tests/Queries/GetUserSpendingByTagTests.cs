#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

/// <summary>
/// The user-scoped sibling of <see cref="GetByTagReportTests"/>: it spans every transactional
/// account the user can reach rather than one named account, and attributes amounts the same way.
/// </summary>
[Trait("Category", "Unit")]
public class GetUserSpendingByTagTests
{
    private static readonly Guid _accountId = Guid.NewGuid();

    private static readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        EmailAddress = "test@test.com",
        FamilyId = Guid.NewGuid(),
        Currency = "AUD",
    };

    /// <summary>
    /// Given a tag marked exclude-from-reporting
    /// When the user's spending by tag is produced
    /// Then spending attributed to that tag is left out
    /// </summary>
    [Fact]
    public async Task Handle_TagExcludedFromReporting_OmitsThatTag()
    {
        // Arrange
        var groceries = CreateTag(1, "Groceries");
        var transfer = CreateTag(2, "Transfer", excludeFromReporting: true);

        var handler = CreateHandler(
        [
            CreateTransaction(-100m, DateTime.Today.AddDays(-5), [groceries]),
            CreateTransaction(-500m, DateTime.Today.AddDays(-3), [transfer]),
        ]);

        // Act
        var result = await handler.Handle(new GetUserSpendingByTag(), TestContext.Current.CancellationToken);

        // Assert
        Assert.DoesNotContain(result.Tags, t => t.TagName == "Transfer");
        Assert.Contains(result.Tags, t => t.TagName == "Groceries" && t.GrossAmount == 100m);
    }

    /// <summary>
    /// Given no tag is excluded
    /// When the user's spending by tag is produced
    /// Then every tag is reported
    /// </summary>
    [Fact]
    public async Task Handle_NoTagExcluded_ReportsEveryTag()
    {
        // Arrange
        var groceries = CreateTag(1, "Groceries");
        var utilities = CreateTag(2, "Utilities");

        var handler = CreateHandler(
        [
            CreateTransaction(-100m, DateTime.Today.AddDays(-5), [groceries]),
            CreateTransaction(-75m, DateTime.Today.AddDays(-3), [utilities]),
        ]);

        // Act
        var result = await handler.Handle(new GetUserSpendingByTag(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(result.Tags, t => t.TagName == "Groceries" && t.GrossAmount == 100m);
        Assert.Contains(result.Tags, t => t.TagName == "Utilities" && t.GrossAmount == 75m);
    }

    private static GetUserSpendingByTagHandler CreateHandler(IEnumerable<DomainTransaction> transactions) =>
        new(QueryableHelper.CreateAsyncQueryable([CreateAccount()]),
            QueryableHelper.CreateAsyncQueryable(transactions),
            _user);

    /// <summary>A transactional account the user owns outright, so it is in scope.</summary>
    private static LogicalAccount CreateAccount() =>
        new(_accountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            AccountType = AccountType.Transaction,
            ShareWithFamily = false,
            Owners = [new InstrumentOwner { InstrumentId = _accountId, UserId = _user.Id }],
        };

    private static DomainTag CreateTag(int id, string name, bool excludeFromReporting = false)
    {
        var tag = new DomainTag(id)
        {
            Name = name,
            FamilyId = _user.FamilyId,
        };

        tag.Settings.ExcludeFromReporting = excludeFromReporting;

        return tag;
    }

    private static DomainTransaction CreateTransaction(decimal amount, DateTime transactionTime, IEnumerable<DomainTag> tags)
    {
        var transactionId = Guid.NewGuid();
        var transaction = new DomainTransaction(transactionId)
        {
            AccountId = _accountId,
            Amount = amount,
            TransactionTime = transactionTime,
            TransactionType = TransactionType.Debit,
            Source = "Test",
            ExcludeFromReporting = false,
        };

        var split = new DomainTransactionSplit(Guid.NewGuid())
        {
            TransactionId = transactionId,
            Amount = Math.Abs(amount),
            Tags = [.. tags],
        };

        var splitsField = typeof(DomainTransaction).GetField("_splits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var splitsList = (List<DomainTransactionSplit>)splitsField!.GetValue(transaction)!;
        splitsList.Add(split);

        return transaction;
    }
}
