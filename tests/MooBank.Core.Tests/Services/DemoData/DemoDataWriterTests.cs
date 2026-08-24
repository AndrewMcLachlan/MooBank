using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Utility;
using Asm.MooBank.Services;
using Asm.MooBank.Services.DemoData;
using Asm.Testing.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Core.Tests.Services.DemoData;

/// <summary>
/// Unit tests for <see cref="DemoDataWriter"/>, which writes one month of demo data.
/// </summary>
public class DemoDataWriterTests
{
    private static readonly Guid CheckingId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IRunRulesService> _runRules = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    /// <summary>
    /// Given the checking account already holds transactions in the month being written
    /// When the writer runs
    /// Then nothing is written.
    /// </summary>
    /// <remarks>
    /// This is the whole idempotency story. The generator's randomness is unseeded, so a second run
    /// produces a different month rather than the same one, and the importer's duplicate detection
    /// would not recognise it -- the account would quietly end up with two months of spending.
    /// </remarks>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_MonthAlreadyWritten_WritesNothing()
    {
        var month = new DateOnly(2026, 7, 1);
        var alreadyThere = DomainTransaction.Create(
            CheckingId, null, -12.50m, "COFFEE", new DateTime(2026, 7, 3), null, "test", null);

        var writer = CreateWriter([alreadyThere]);

        await writer.Extend(month, TestContext.Current.CancellationToken);

        _transactionRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Never);
        _runRules.Verify(r => r.RunRules(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given the checking account's transactions all fall outside the month being written
    /// When the writer runs
    /// Then the month is generated and the account's rules are run over it.
    /// </summary>
    /// <remarks>
    /// The mirror of the guard above -- a neighbouring month must not be mistaken for this one, or
    /// the demo would never move again -- and it pins the tagging route: rules, not anything the
    /// job decides for itself.
    /// </remarks>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_MonthEmpty_GeneratesAndRunsRules()
    {
        var month = new DateOnly(2026, 7, 1);
        var older = DomainTransaction.Create(
            CheckingId, null, -12.50m, "COFFEE", new DateTime(2026, 5, 3), null, "test", null);

        var writer = CreateWriter([older]);

        await writer.Extend(month, TestContext.Current.CancellationToken);

        _transactionRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.AtLeastOnce);
        _runRules.Verify(r => r.RunRules(CheckingId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given generation produces transactions
    /// When the writer runs
    /// Then every one of them falls inside the month being written.
    /// </summary>
    /// <remarks>
    /// The generator is handed a window rather than trusted to respect one, so this checks the
    /// window is the month asked for. A transaction landing outside it would defeat the occupancy
    /// guard on the next run.
    /// </remarks>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Extend_MonthEmpty_WritesOnlyWithinTheMonth()
    {
        var month = new DateOnly(2026, 7, 1);
        var added = new List<DomainTransaction>();
        _transactionRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>())).Callback<DomainTransaction>(added.Add);

        var writer = CreateWriter([]);

        await writer.Extend(month, TestContext.Current.CancellationToken);

        Assert.NotEmpty(added);
        Assert.All(added, t =>
        {
            Assert.True(t.TransactionTime >= new DateTime(2026, 7, 1), $"{t.TransactionTime} is before the month");
            Assert.True(t.TransactionTime < new DateTime(2026, 8, 1), $"{t.TransactionTime} is after the month");
        });
    }

    private DemoDataWriter CreateWriter(IEnumerable<DomainTransaction> transactions) =>
        new(
            Options.Create(new DemoDataOptions { CheckingAccountId = CheckingId }),
            MockDbSetFactory.CreateQueryable(transactions),
            MockDbSetFactory.CreateQueryable<TagEntity>([]),
            MockDbSetFactory.CreateQueryable<LogicalAccount>([]),
            _transactionRepository.Object,
            Mock.Of<IAccountRepository>(),
            _runRules.Object,
            _unitOfWork.Object,
            NullLogger<DemoDataWriter>.Instance);
}
