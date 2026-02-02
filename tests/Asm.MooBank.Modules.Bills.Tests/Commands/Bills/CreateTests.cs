#nullable enable
using Asm.MooBank.Modules.Bills.Commands.Bills;
using Asm.MooBank.Modules.Bills.Models;
using Asm.MooBank.Modules.Bills.Tests.Support;
using DomainAccount = Asm.MooBank.Domain.Entities.Utility.Account;

namespace Asm.MooBank.Modules.Bills.Tests.Commands.Bills;

[Trait("Category", "Unit")]
public class CreateTests()
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsBill()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var createBill = new CreateBill
        {
            InvoiceNumber = "INV001",
            IssueDate = new DateOnly(2024, 6, 15),
            CurrentReading = 5000,
            PreviousReading = 4500,
            Total = 500,
            Cost = 150.50m,
            CostsIncludeGST = true,
            Periods = [],
            Discounts = [],
        };
        var command = new Create(accountId, createBill);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV001", result.InvoiceNumber);
        Assert.Equal(new DateOnly(2024, 6, 15), result.IssueDate);
        Assert.Equal(5000, result.CurrentReading);
        Assert.Equal(4500, result.PreviousReading);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsBillToAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var createBill = new CreateBill
        {
            InvoiceNumber = "INV002",
            IssueDate = new DateOnly(2024, 7, 1),
            Periods = [],
            Discounts = [],
        };
        var command = new Create(accountId, createBill);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(account.Bills);
        var addedBill = account.Bills.First();
        Assert.Equal("INV002", addedBill.InvoiceNumber);
        Assert.Equal(accountId, addedBill.AccountId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var createBill = new CreateBill
        {
            IssueDate = new DateOnly(2024, 1, 1),
            Periods = [],
            Discounts = [],
        };
        var command = new Create(accountId, createBill);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentAccount_ThrowsNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<DomainAccount>(null!));

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var createBill = new CreateBill
        {
            IssueDate = new DateOnly(2024, 1, 1),
            Periods = [],
            Discounts = [],
        };
        var command = new Create(accountId, createBill);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_WithPeriods_CreatesPeriods()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var createBill = new CreateBill
        {
            IssueDate = new DateOnly(2024, 6, 1),
            Periods =
            [
                new Period
                {
                    PeriodStart = new DateTime(2024, 4, 1),
                    PeriodEnd = new DateTime(2024, 4, 30),
                    ChargePerDay = 1.5m,
                    PricePerUnit = 0.25m,
                    TotalUsage = 450,
                },
                new Period
                {
                    PeriodStart = new DateTime(2024, 5, 1),
                    PeriodEnd = new DateTime(2024, 5, 31),
                    ChargePerDay = 1.5m,
                    PricePerUnit = 0.28m,
                    TotalUsage = 520,
                },
            ],
            Discounts = [],
        };
        var command = new Create(accountId, createBill);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var addedBill = account.Bills.First();
        Assert.Equal(2, addedBill.Periods.Count);
    }

    [Fact]
    public async Task Handle_WithDiscounts_CreatesDiscounts()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var createBill = new CreateBill
        {
            IssueDate = new DateOnly(2024, 6, 1),
            Periods = [],
            Discounts =
            [
                new Discount
                {
                    DiscountPercent = 10,
                    Reason = "Early payment",
                },
                new Discount
                {
                    DiscountAmount = 25.00m,
                    Reason = "Loyalty bonus",
                },
            ],
        };
        var command = new Create(accountId, createBill);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var addedBill = account.Bills.First();
        Assert.Equal(2, addedBill.Discounts.Count);
    }

    [Fact]
    public async Task Handle_FullBillDetails_CreatesCompleteEntity()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateAccount(id: accountId);

        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var createBill = new CreateBill
        {
            InvoiceNumber = "FULL001",
            IssueDate = new DateOnly(2024, 6, 15),
            CurrentReading = 12500,
            PreviousReading = 12000,
            Total = 500,
            Cost = 175.50m,
            CostsIncludeGST = true,
            Periods =
            [
                new Period
                {
                    PeriodStart = new DateTime(2024, 5, 15),
                    PeriodEnd = new DateTime(2024, 6, 14),
                    ChargePerDay = 1.25m,
                    PricePerUnit = 0.30m,
                    TotalUsage = 500,
                },
            ],
            Discounts =
            [
                new Discount
                {
                    DiscountPercent = 5,
                    Reason = "Online payment",
                },
            ],
        };
        var command = new Create(accountId, createBill);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var bill = account.Bills.First();
        Assert.Equal("FULL001", bill.InvoiceNumber);
        Assert.Equal(12500, bill.CurrentReading);
        Assert.Equal(12000, bill.PreviousReading);
        Assert.True(bill.CostsIncludeGST);
        Assert.Single(bill.Periods);
        Assert.Single(bill.Discounts);
    }
}
