using Asm.MooBank.Modules.Accounts.Queries.InstitutionAccounts;
using Asm.MooBank.Modules.Accounts.Tests.Support;

namespace Asm.MooBank.Modules.Accounts.Tests.Queries.InstitutionAccounts;

[Trait("Category", "Unit")]
public class GetTests
{
    [Fact]
    public async Task Handle_ExistingInstitutionAccount_ReturnsInstitutionAccount()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: institutionAccountId,
            instrumentId: logicalAccountId,
            name: "Test Account",
            institutionId: 1);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: [institutionAccount]);
        var queryable = TestEntities.CreateLogicalAccountQueryable(logicalAccount);

        var handler = new GetHandler(queryable);
        var query = new Get(logicalAccountId, institutionAccountId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(institutionAccountId, result.Id);
        Assert.Equal("Test Account", result.Name);
        Assert.Equal(1, result.InstitutionId);
    }

    [Fact]
    public async Task Handle_MultipleInstitutionAccounts_ReturnsCorrectOne()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var institutionAccounts = new[]
        {
            TestEntities.CreateInstitutionAccount(instrumentId: logicalAccountId, name: "First Account"),
            TestEntities.CreateInstitutionAccount(id: targetId, instrumentId: logicalAccountId, name: "Target Account"),
            TestEntities.CreateInstitutionAccount(instrumentId: logicalAccountId, name: "Third Account"),
        };
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: institutionAccounts);
        var queryable = TestEntities.CreateLogicalAccountQueryable(logicalAccount);

        var handler = new GetHandler(queryable);
        var query = new Get(logicalAccountId, targetId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetId, result.Id);
        Assert.Equal("Target Account", result.Name);
    }

    [Fact]
    public async Task Handle_NonExistentLogicalAccount_ThrowsNotFoundException()
    {
        // Arrange
        var logicalAccount = TestEntities.CreateLogicalAccount();
        var queryable = TestEntities.CreateLogicalAccountQueryable(logicalAccount);

        var handler = new GetHandler(queryable);
        var query = new Get(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NonExistentInstitutionAccount_ThrowsNotFoundException()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var institutionAccount = TestEntities.CreateInstitutionAccount(instrumentId: logicalAccountId);
        var logicalAccount = TestEntities.CreateLogicalAccount(
            id: logicalAccountId,
            institutionAccounts: [institutionAccount]);
        var queryable = TestEntities.CreateLogicalAccountQueryable(logicalAccount);

        var handler = new GetHandler(queryable);
        var query = new Get(logicalAccountId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_EmptyInstitutionAccounts_ThrowsNotFoundException()
    {
        // Arrange
        var logicalAccountId = Guid.NewGuid();
        var logicalAccount = TestEntities.CreateLogicalAccount(id: logicalAccountId);
        var queryable = TestEntities.CreateLogicalAccountQueryable(logicalAccount);

        var handler = new GetHandler(queryable);
        var query = new Get(logicalAccountId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }
}
