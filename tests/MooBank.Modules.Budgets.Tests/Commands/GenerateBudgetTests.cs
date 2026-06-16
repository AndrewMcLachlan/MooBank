#nullable enable
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainBudgetLine = Asm.MooBank.Domain.Entities.Budget.BudgetLine;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Budgets.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="GenerateBudgetHandler"/> — orchestration and the
/// fill-gaps / exclusion behaviour (classification itself is covered by
/// <see cref="Asm.MooBank.Modules.Budgets.Services.BudgetSuggestionCalculator"/> tests).
/// </summary>
[Trait("Category", "Unit")]
[Collection("DbFunction Tests")]
public class GenerateBudgetTests : IDisposable
{
    private readonly TestMocks _mocks = new();
    private readonly IQueryable<TagRelationship> relationshipQueryable = TestEntities.CreateTagRelationshipQueryable();

    public GenerateBudgetTests()
    {
        // Default net: debits negative, credits positive, no offsets (non-zero amounts stay non-zero).
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
            type == TransactionType.Debit ? -Math.Abs(amount) : Math.Abs(amount));
    }

    public void Dispose() => DomainTransaction.ResetTransactionNetAmountOverride();

    private static DomainTransaction MonthlyExpense(Guid accountId, DomainTag tag, int monthsAgo, decimal amount)
    {
        var anchor = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 15).AddMonths(-monthsAgo);
        var split = TestEntities.CreateTransactionSplit(amount: amount, tags: [tag]);
        return TestEntities.CreateTransaction(accountId: accountId, amount: -amount, transactionTime: anchor, splits: [split]);
    }

    private static DomainTransaction MonthlyCredit(Guid accountId, DomainTag tag, int monthsAgo, decimal amount)
    {
        var anchor = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 15).AddMonths(-monthsAgo);
        var split = TestEntities.CreateTransactionSplit(amount: amount, tags: [tag]);
        return TestEntities.CreateTransaction(accountId: accountId, amount: amount, transactionTime: anchor, splits: [split]);
    }

    [Fact]
    public async Task Handle_NewMonthlyTag_AddsAnExpenseLine()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var groceries = TestEntities.CreateTag(2, "Groceries");
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyExpense(accountId, groceries, i, 100m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { groceries });

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Groceries"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            relationshipQueryable, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert
        var line = Assert.Single(captured);
        Assert.Equal(2, line.TagId);
        Assert.False(line.Income);
        Assert.Equal(BudgetSuggestionCalculatorMonthMask, line.Month);
        Assert.Equal(100m, line.Amount);
        Assert.StartsWith("Auto:", line.Notes);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TagAlreadyBudgeted_IsSkipped()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var existingLine = TestEntities.CreateBudgetLine(tagId: 2, tagName: "Groceries", amount: 500m);
        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId, lines: [existingLine]);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var groceries = TestEntities.CreateTag(2, "Groceries");
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyExpense(accountId, groceries, i, 100m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { groceries });

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            relationshipQueryable, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — nothing added because the only spending tag is already budgeted.
        _mocks.BudgetRepositoryMock.Verify(r => r.AddLine(It.IsAny<DomainBudgetLine>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TagExcludedFromReporting_IsSkipped()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var excludedTag = TestEntities.CreateTag(7, "Transfers", settings: new TagSettings(7) { ExcludeFromReporting = true });
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyExpense(accountId, excludedTag, i, 100m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { excludedTag });

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            relationshipQueryable, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert
        _mocks.BudgetRepositoryMock.Verify(r => r.AddLine(It.IsAny<DomainBudgetLine>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RefundsOnExpenseTag_NetToExpenseNotIncome()
    {
        // Arrange — a spending tag with the occasional refund (a credit). The refunds
        // should reduce the expense, not create a bogus income line.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var shopping = TestEntities.CreateTag(2, "Shopping");
        var transactions = Enumerable.Range(1, 12).Select(i => MonthlyExpense(accountId, shopping, i, 100m))
            .Concat(Enumerable.Range(1, 3).Select(i => MonthlyCredit(accountId, shopping, i, 50m)))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { shopping });

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Shopping"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            relationshipQueryable, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — exactly one line, and it's an expense (no income line from the refunds).
        var line = Assert.Single(captured);
        Assert.Equal(2, line.TagId);
        Assert.False(line.Income);
    }

    [Fact]
    public async Task Handle_GenuineIncome_CreatesIncomeLine()
    {
        // Arrange — a tag that is purely credits (e.g. salary) should produce an income line.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var salary = TestEntities.CreateTag(3, "Salary");
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyCredit(accountId, salary, i, 5000m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { salary });

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Salary"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            relationshipQueryable, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert
        var line = Assert.Single(captured);
        Assert.Equal(3, line.TagId);
        Assert.True(line.Income);
        Assert.Equal(5000m, line.Amount);
    }

    [Fact]
    public async Task Handle_FullyOffsetTransactions_AreExcluded()
    {
        // Arrange — the House Insurance case: tagged money moves most months, but those
        // entries are fully offset (net zero); only one real payment is made each year.
        // The offset entries must be ignored so it's detected as a yearly payment, not a
        // ~monthly expense.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        // Offset (net-zero) entries of $300 net to zero; the real $1500 March payments don't.
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
            Math.Abs(amount) == 300m ? 0m : (type == TransactionType.Debit ? -Math.Abs(amount) : Math.Abs(amount)));

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var houseInsurance = TestEntities.CreateTag(35, "House Insurance");

        DomainTransaction MarchPayment(int year) =>
            TestEntities.CreateTransaction(accountId: accountId, amount: -1500m, transactionTime: new DateTime(year, 3, 15),
                splits: [TestEntities.CreateTransactionSplit(amount: 1500m, tags: [houseInsurance])]);

        var transactions = Enumerable.Range(1, 18)                          // monthly offset noise across the window
            .Select(i => MonthlyExpense(accountId, houseInsurance, i, 300m))
            .Append(MarchPayment(2025))                                     // the two real annual payments
            .Append(MarchPayment(2026))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { houseInsurance });

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "House Insurance"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            relationshipQueryable, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — one yearly line in March at the full payment, NOT a ~monthly amount.
        var line = Assert.Single(captured);
        Assert.False(line.Income);
        Assert.Equal((short)(1 << 2), line.Month); // March only
        Assert.Equal(1500m, line.Amount);
        Assert.Contains("yearly", line.Notes);
    }

    [Fact]
    public async Task Handle_LeafSpend_RollsUpToMarkedCategoryAncestor()
    {
        // Arrange — "Insurance" (33) is marked a budget category; spend is tagged at the leaf
        // "House Insurance" (35). Generation should create a line for the category ancestor.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var insurance = TestEntities.CreateTag(33, "Insurance", settings: new TagSettings(33) { BudgetCategory = true });
        var houseInsurance = TestEntities.CreateTag(35, "House Insurance");
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyExpense(accountId, houseInsurance, i, 100m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { insurance, houseInsurance });
        var rels = TestEntities.CreateTagRelationshipQueryable(TestEntities.CreateTagRelationship(tagId: 35, parentTagId: 33));

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Insurance"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            rels, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — one line, for the category ancestor tag 33, not the leaf 35.
        var line = Assert.Single(captured);
        Assert.Equal(33, line.TagId);
    }

    [Fact]
    public async Task Handle_MarkedCategories_RollUpDescendantsButKeepMarkedChildrenSeparate()
    {
        // Arrange — the bespoke case: Medical (29) and Pharmacy (53) are budget categories.
        // Dental (95), a child of Medical, rolls up to Medical; Pharmacy — also a child of
        // Medical but marked itself — stays as its own line.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var medical = TestEntities.CreateTag(29, "Medical", settings: new TagSettings(29) { BudgetCategory = true });
        var pharmacy = TestEntities.CreateTag(53, "Pharmacy", settings: new TagSettings(53) { BudgetCategory = true });
        var dental = TestEntities.CreateTag(95, "Dental");

        var transactions = Enumerable.Range(1, 12).Select(i => MonthlyExpense(accountId, dental, i, 100m))
            .Concat(Enumerable.Range(1, 12).Select(i => MonthlyExpense(accountId, pharmacy, i, 80m)))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { medical, pharmacy, dental });
        var rels = TestEntities.CreateTagRelationshipQueryable(
            TestEntities.CreateTagRelationship(tagId: 95, parentTagId: 29),
            TestEntities.CreateTagRelationship(tagId: 53, parentTagId: 29));

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Tag"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            rels, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — a line for Medical (29, Dental rolled in) and one for Pharmacy (53); never the leaf 95.
        var ids = captured.Select(l => l.TagId).OrderBy(x => x).ToList();
        Assert.Equal([29, 53], ids);
    }

    [Fact]
    public async Task Handle_NoBudgetedAncestor_KeepsTagAtItsOwnLevel()
    {
        // Arrange — spend on a tag whose ancestors were never budgeted should NOT be rolled
        // all the way up to a top-level category; it stays at its own level.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var medical = TestEntities.CreateTag(29, "Medical");
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyExpense(accountId, medical, i, 100m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { medical });
        // Medical (29) sits under a never-budgeted "Living Expenses" (200).
        var rels = TestEntities.CreateTagRelationshipQueryable(TestEntities.CreateTagRelationship(tagId: 29, parentTagId: 200));

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Medical"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            rels, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — stays at Medical (29), not rolled up to 200.
        var line = Assert.Single(captured);
        Assert.Equal(29, line.TagId);
    }

    [Fact]
    public async Task Handle_PreviouslyBudgetedAncestor_IsARollupTargetWithoutMarking()
    {
        // Arrange — "Insurance" (33) is NOT marked a category, but the family budgeted it in a
        // prior year. It should still act as a rollup target (the pre-tag-settings behaviour),
        // so House Insurance (35) rolls up to it.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var priorYear = TestEntities.CreateBudget(year: 2025, familyId: familyId,
            lines: [TestEntities.CreateBudgetLine(tagId: 33, tagName: "Insurance", amount: 200m)]);
        var thisYear = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(priorYear, thisYear);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var insurance = TestEntities.CreateTag(33, "Insurance");   // not marked
        var houseInsurance = TestEntities.CreateTag(35, "House Insurance");
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyExpense(accountId, houseInsurance, i, 100m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { insurance, houseInsurance });
        var rels = TestEntities.CreateTagRelationshipQueryable(TestEntities.CreateTagRelationship(tagId: 35, parentTagId: 33));

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thisYear);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Insurance"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            rels, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — rolled up to the previously-budgeted ancestor 33, despite no marking.
        var line = Assert.Single(captured);
        Assert.Equal(33, line.TagId);
    }

    [Fact]
    public async Task Handle_DeepLeaf_RollsUpToNearestCategoryAcrossClosure()
    {
        // Arrange — mirrors the real TagHierarchies closure: Paediatrics (144) sits under
        // Medical Specialists (179) under Medical (29) under Living Expense (200). The closure
        // lists every ancestor with Ordinal topmost-first (200=1, 29=2, 179=3). Only Medical
        // (29) is a budget category, so a deep leaf must roll up to Medical — not to the
        // topmost ancestor, and not stay at the leaf.
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2026, familyId: familyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(
            TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true));

        var medical = TestEntities.CreateTag(29, "Medical", settings: new TagSettings(29) { BudgetCategory = true });
        var paediatrics = TestEntities.CreateTag(144, "Paediatrics");
        var transactions = Enumerable.Range(1, 12)
            .Select(i => MonthlyExpense(accountId, paediatrics, i, 100m))
            .ToArray();
        var transactionQueryable = TestEntities.CreateTransactionQueryable(transactions);
        var tagQueryable = QueryableHelper.CreateAsyncQueryable(new[] { medical, paediatrics });
        var rels = TestEntities.CreateTagRelationshipQueryable(
            TestEntities.CreateTagRelationship(tagId: 144, parentTagId: 200, ordinal: 1),  // Living Expense (topmost)
            TestEntities.CreateTagRelationship(tagId: 144, parentTagId: 29, ordinal: 2),   // Medical
            TestEntities.CreateTagRelationship(tagId: 144, parentTagId: 179, ordinal: 3)); // Medical Specialists (nearest)

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(familyId, (short)2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var captured = new List<DomainBudgetLine>();
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => { l.Tag = TestEntities.CreateTag(l.TagId, "Medical"); captured.Add(l); })
            .Returns<DomainBudgetLine>(l => l);

        var handler = new GenerateBudgetHandler(budgetQueryable, accountQueryable, transactionQueryable, tagQueryable,
            rels, _mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.User);

        // Act
        await handler.Handle(new GenerateBudget(2026), TestContext.Current.CancellationToken);

        // Assert — rolled up to Medical (29), not Living Expense (200) and not the leaf (144).
        var line = Assert.Single(captured);
        Assert.Equal(29, line.TagId);
    }

    private const short BudgetSuggestionCalculatorMonthMask = 4095;
}
