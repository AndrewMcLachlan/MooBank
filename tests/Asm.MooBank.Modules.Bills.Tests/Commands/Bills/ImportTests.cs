#nullable enable
using Asm.MooBank.Modules.Bills.Commands.Bills;
using Asm.MooBank.Modules.Bills.Tests.Support;
using DomainAccount = Asm.MooBank.Domain.Entities.Utility.Account;

namespace Asm.MooBank.Modules.Bills.Tests.Commands.Bills;

[Trait("Category", "Unit")]
public class ImportTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_ValidBills_ImportsAllBills()
    {
        // Arrange
        var account = TestEntities.CreateAccount(name: "Test Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account", issueDate: new DateOnly(2024, 1, 15)),
            TestEntities.CreateImportBill(accountName: "Test Account", issueDate: new DateOnly(2024, 2, 15)),
        };
        var command = new Import(bills);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Imported);
        Assert.Equal(0, result.Failed);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Handle_ValidBills_AddsBillsToAccount()
    {
        // Arrange
        var account = TestEntities.CreateAccount(name: "Test Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account", issueDate: new DateOnly(2024, 1, 15)),
        };
        var command = new Import(bills);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(account.Bills);
    }

    [Fact]
    public async Task Handle_ValidBills_SavesChanges()
    {
        // Arrange
        var account = TestEntities.CreateAccount(name: "Test Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account"),
        };
        var command = new Import(bills);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AccountNotFound_ReturnsError()
    {
        // Arrange
        var account = TestEntities.CreateAccount(name: "Existing Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Non Existent Account", issueDate: new DateOnly(2024, 1, 15)),
        };
        var command = new Import(bills);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Failed);
        Assert.Contains(result.Errors, e => e.Contains("Non Existent Account") && e.Contains("not found"));
    }

    [Fact]
    public async Task Handle_DuplicateInvoiceNumber_ReturnsError()
    {
        // Arrange
        var existingBill = TestEntities.CreateBill(invoiceNumber: "INV001");
        var account = TestEntities.CreateAccount(name: "Test Account", bills: [existingBill]);
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account", invoiceNumber: "INV001"),
        };
        var command = new Import(bills);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Failed);
        Assert.Contains(result.Errors, e => e.Contains("INV001") && e.Contains("already exists"));
    }

    [Fact]
    public async Task Handle_DuplicateIssueDate_ReturnsError()
    {
        // Arrange
        var existingBill = TestEntities.CreateBill(issueDate: new DateOnly(2024, 1, 15), invoiceNumber: null);
        var account = TestEntities.CreateAccount(name: "Test Account", bills: [existingBill]);
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account", issueDate: new DateOnly(2024, 1, 15), invoiceNumber: null),
        };
        var command = new Import(bills);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Failed);
        var expectedDate = new DateOnly(2024, 1, 15).ToString("d");
        Assert.Contains(result.Errors, e => e.Contains(expectedDate) && e.Contains("already exists"));
    }

    [Fact]
    public async Task Handle_MixedResults_ReturnsCorrectCounts()
    {
        // Arrange
        var existingBill = TestEntities.CreateBill(invoiceNumber: "EXISTING");
        var account = TestEntities.CreateAccount(name: "Test Account", bills: [existingBill]);
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account", issueDate: new DateOnly(2024, 1, 15), invoiceNumber: "NEW1"),
            TestEntities.CreateImportBill(accountName: "Test Account", issueDate: new DateOnly(2024, 2, 15), invoiceNumber: "EXISTING"),
            TestEntities.CreateImportBill(accountName: "Wrong Account", issueDate: new DateOnly(2024, 3, 15)),
            TestEntities.CreateImportBill(accountName: "Test Account", issueDate: new DateOnly(2024, 4, 15), invoiceNumber: "NEW2"),
        };
        var command = new Import(bills);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Imported);
        Assert.Equal(2, result.Failed);
        Assert.Equal(2, result.Errors.Count());
    }

    [Fact]
    public async Task Handle_WithPeriods_CreatesPeriods()
    {
        // Arrange
        var account = TestEntities.CreateAccount(name: "Test Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var periods = new[]
        {
            TestEntities.CreateModelPeriod(periodStart: new DateTime(2024, 1, 1), periodEnd: new DateTime(2024, 1, 31)),
            TestEntities.CreateModelPeriod(periodStart: new DateTime(2024, 2, 1), periodEnd: new DateTime(2024, 2, 28)),
        };
        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account", periods: periods),
        };
        var command = new Import(bills);

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
        var account = TestEntities.CreateAccount(name: "Test Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var discounts = new[]
        {
            TestEntities.CreateModelDiscount(discountPercent: 10, reason: "Early payment"),
            TestEntities.CreateModelDiscount(discountAmount: 25m, reason: "Loyalty"),
        };
        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "Test Account", discounts: discounts),
        };
        var command = new Import(bills);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var addedBill = account.Bills.First();
        Assert.Equal(2, addedBill.Discounts.Count);
    }

    [Fact]
    public async Task Handle_AccountNameCaseInsensitive_MatchesAccount()
    {
        // Arrange
        var account = TestEntities.CreateAccount(name: "Test Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var bills = new[]
        {
            TestEntities.CreateImportBill(accountName: "TEST ACCOUNT"),
        };
        var command = new Import(bills);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Imported);
        Assert.Single(account.Bills);
    }

    [Fact]
    public async Task Handle_EmptyBillsList_ImportsNothing()
    {
        // Arrange
        var account = TestEntities.CreateAccount(name: "Test Account");
        _mocks.AccountRepositoryMock
            .Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var handler = new ImportHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object);

        var command = new Import([]);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Imported);
        Assert.Equal(0, result.Failed);
        Assert.Empty(result.Errors);
    }
}
