#nullable enable
using Asm.MooBank.Modules.Bills.Commands.Bills;
using Asm.MooBank.Modules.Bills.Models;
using Asm.MooBank.Modules.Bills.Tests.Support;
using DomainAccount = Asm.MooBank.Domain.Entities.Utility.Account;
using DomainPeriod = Asm.MooBank.Domain.Entities.Utility.Period;

namespace Asm.MooBank.Modules.Bills.Tests.Commands.Bills;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private const int BillId = 7;

    private readonly TestMocks _mocks = new();

    /// <summary>
    /// Given a bill with a consumption period
    /// When it is updated with that period plus an export
    /// Then the bill carries both, which is the point of the whole feature.
    /// </summary>
    [Fact]
    public async Task Handle_ExportAdded_BillCarriesConsumptionAndExport()
    {
        var (handler, account) = Arrange();

        var command = new Update(account.Id, BillId, new UpdateBill
        {
            IssueDate = new DateOnly(2026, 6, 15),
            Periods =
            [
                new Period
                {
                    PeriodStart = new DateTime(2026, 5, 16),
                    PeriodEnd = new DateTime(2026, 6, 15),
                    ServiceCharges = [new ServiceCharge { ChargeTypeId = 1, ChargePerDay = 1.10m }],
                    Usages =
                    [
                        new Usage { UsageType = UsageType.Consumption, PricePerUnit = 0.30m, TotalUsage = 400 },
                        new Usage { UsageType = UsageType.Export, PricePerUnit = 0.05m, TotalUsage = 250 },
                    ],
                },
            ],
        });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        var usages = result.Periods.Single().Usages.ToList();
        Assert.Equal(2, usages.Count);
        Assert.Contains(usages, u => u.UsageType == UsageType.Consumption && u.TotalUsage == 400);
        Assert.Contains(usages, u => u.UsageType == UsageType.Export && u.TotalUsage == 250);
    }

    /// <summary>
    /// Given a bill with existing periods
    /// When it is updated
    /// Then the periods supplied replace them rather than being appended.
    /// </summary>
    /// <remarks>
    /// Appending would silently double a bill's cost, because every period's charges are summed.
    /// </remarks>
    [Fact]
    public async Task Handle_PeriodsSupplied_ReplacesRatherThanAppends()
    {
        var (handler, account) = Arrange();

        Assert.Single(account.Bills.Single().Periods);

        var command = new Update(account.Id, BillId, new UpdateBill
        {
            IssueDate = new DateOnly(2026, 6, 15),
            Periods =
            [
                new Period
                {
                    PeriodStart = new DateTime(2026, 5, 16),
                    PeriodEnd = new DateTime(2026, 6, 15),
                    Usages = [new Usage { UsageType = UsageType.Consumption, PricePerUnit = 0.30m, TotalUsage = 400 }],
                },
            ],
        });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Single(account.Bills.Single().Periods);
    }

    /// <summary>
    /// Given a bill
    /// When its scalar details are updated
    /// Then they are written to the entity.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_UpdatesTheBillDetails()
    {
        var (handler, account) = Arrange();

        var command = new Update(account.Id, BillId, new UpdateBill
        {
            InvoiceNumber = "INV-999",
            IssueDate = new DateOnly(2026, 6, 15),
            CurrentReading = 8000,
            PreviousReading = 7600,
            CostsIncludeGST = false,
            Periods = [],
        });

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal("INV-999", result.InvoiceNumber);
        Assert.Equal(new DateOnly(2026, 6, 15), result.IssueDate);
        Assert.Equal(8000, result.CurrentReading);
        Assert.Equal(7600, result.PreviousReading);
        Assert.False(result.CostsIncludeGST);
    }

    /// <summary>
    /// Given a valid update
    /// When it is handled
    /// Then the change is committed.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        var (handler, account) = Arrange();

        var command = new Update(account.Id, BillId, new UpdateBill { IssueDate = new DateOnly(2026, 6, 15), Periods = [] });

        await handler.Handle(command, TestContext.Current.CancellationToken);

        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given an account the user cannot see, or one that does not exist
    /// When a bill on it is updated
    /// Then the update is refused rather than silently doing nothing.
    /// </summary>
    [Fact]
    public async Task Handle_AccountNotFound_Throws()
    {
        var accountId = Guid.NewGuid();

        _mocks.AccountRepositoryMock
            .Setup(r => r.GetWithBill(accountId, BillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainAccount?)null);

        var handler = new UpdateHandler(_mocks.UnitOfWorkMock.Object, _mocks.AccountRepositoryMock.Object);

        var command = new Update(accountId, BillId, new UpdateBill { IssueDate = new DateOnly(2026, 6, 15) });

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    /// <summary>
    /// Given an account that does not carry the bill
    /// When the bill is updated
    /// Then the update is refused.
    /// </summary>
    [Fact]
    public async Task Handle_BillNotOnAccount_Throws()
    {
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);

        _mocks.AccountRepositoryMock
            .Setup(r => r.GetWithBill(accountId, BillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new UpdateHandler(_mocks.UnitOfWorkMock.Object, _mocks.AccountRepositoryMock.Object);

        var command = new Update(accountId, BillId, new UpdateBill { IssueDate = new DateOnly(2026, 6, 15) });

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    private (UpdateHandler Handler, DomainAccount Account) Arrange()
    {
        var accountId = Guid.NewGuid();
        var bill = TestEntities.CreateBill(
            id: BillId,
            accountId: accountId,
            invoiceNumber: "INV-001",
            issueDate: new DateOnly(2026, 5, 15),
            periods: [TestEntities.CreatePeriod(id: 1)]);

        var account = TestEntities.CreateAccount(id: accountId, bills: [bill]);

        _mocks.AccountRepositoryMock
            .Setup(r => r.GetWithBill(accountId, BillId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        return (new UpdateHandler(_mocks.UnitOfWorkMock.Object, _mocks.AccountRepositoryMock.Object), account);
    }
}
