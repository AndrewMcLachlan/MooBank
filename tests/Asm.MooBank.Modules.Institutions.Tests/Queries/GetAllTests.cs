using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Queries;
using Asm.MooBank.Modules.Institutions.Tests.Support;

namespace Asm.MooBank.Modules.Institutions.Tests.Queries;

[Trait("Category", "Unit")]
public class GetAllTests
{
    [Fact]
    public async Task Handle_NoFilter_ReturnsAllInstitutionsOrderedByName()
    {
        // Arrange
        var institutions = TestEntities.CreateSampleInstitutions();
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetAllHandler(queryable);
        var query = new GetAll(null);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(6, result.Count);
        Assert.Equal("Alpha Bank", result[0].Name);
        Assert.Equal("Beta Credit Union", result[1].Name);
        Assert.Equal("Delta Super Fund", result[2].Name);
        Assert.Equal("Epsilon Broker", result[3].Name);
        Assert.Equal("Gamma Building Society", result[4].Name);
        Assert.Equal("Zeta Other", result[5].Name);
    }

    [Fact]
    public async Task Handle_EmptyCollection_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreateInstitutionQueryable([]);
        var handler = new GetAllHandler(queryable);
        var query = new GetAll(null);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_SuperannuationFilter_ReturnsOnlySuperFunds()
    {
        // Arrange
        var institutions = TestEntities.CreateSampleInstitutions();
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetAllHandler(queryable);
        var query = new GetAll(AccountType.Superannuation);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Delta Super Fund", result[0].Name);
        Assert.Equal(InstitutionType.SuperannuationFund, result[0].InstitutionType);
    }

    [Fact]
    public async Task Handle_InvestmentFilter_ReturnsOnlyBrokers()
    {
        // Arrange
        var institutions = TestEntities.CreateSampleInstitutions();
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetAllHandler(queryable);
        var query = new GetAll(AccountType.Investment);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Epsilon Broker", result[0].Name);
        Assert.Equal(InstitutionType.Broker, result[0].InstitutionType);
    }

    [Fact]
    public async Task Handle_TransactionFilter_ReturnsBanksAndSimilar()
    {
        // Arrange
        var institutions = TestEntities.CreateSampleInstitutions();
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetAllHandler(queryable);
        var query = new GetAll(AccountType.Transaction);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.All(result, r => Assert.True(
            r.InstitutionType == InstitutionType.Bank ||
            r.InstitutionType == InstitutionType.BuildingSociety ||
            r.InstitutionType == InstitutionType.CreditUnion ||
            r.InstitutionType == InstitutionType.Other));
    }

    [Theory]
    [InlineData(AccountType.Savings)]
    [InlineData(AccountType.Credit)]
    [InlineData(AccountType.Loan)]
    [InlineData(AccountType.Mortgage)]
    public async Task Handle_OtherAccountTypeFilters_ReturnsBanksAndSimilar(AccountType accountType)
    {
        // Arrange
        var institutions = TestEntities.CreateSampleInstitutions();
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetAllHandler(queryable);
        var query = new GetAll(accountType);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.All(result, r => Assert.True(
            r.InstitutionType == InstitutionType.Bank ||
            r.InstitutionType == InstitutionType.BuildingSociety ||
            r.InstitutionType == InstitutionType.CreditUnion ||
            r.InstitutionType == InstitutionType.Other));
    }

    [Fact]
    public async Task Handle_FilterWithNoMatches_ReturnsEmptyList()
    {
        // Arrange - only create non-superannuation institutions
        var institutions = new[]
        {
            TestEntities.CreateInstitution(1, "Test Bank", InstitutionType.Bank),
            TestEntities.CreateInstitution(2, "Test Credit Union", InstitutionType.CreditUnion),
        };
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetAllHandler(queryable);
        var query = new GetAll(AccountType.Superannuation);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        Assert.Empty(result);
    }
}
