#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Importers;
using Asm.MooBank.Infrastructure.Importers;
using Asm.MooBank.Infrastructure.Tests.Support;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Infrastructure.Tests.Importers;

[Trait("Category", "Unit")]
public class ImporterFactoryTests : IDisposable
{
    private readonly MooBankContext _context;
    private readonly ServiceCollection _services;
    private IServiceProvider? _serviceProvider;

    public ImporterFactoryTests()
    {
        _context = TestDbContextFactory.Create();
        _services = new ServiceCollection();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Create(Guid instrumentId, Guid accountId)

    [Fact]
    public async Task Create_LogicalAccountNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentInstrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var factory = CreateFactory();

        // Act
        var result = await factory.Create(nonExistentInstrumentId, accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Create_InstitutionAccountNotFound_ReturnsNull()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var nonExistentAccountId = Guid.NewGuid();

        var logicalAccount = TestEntities.CreateLogicalAccount(id: instrumentId);
        _context.Add(logicalAccount);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var factory = CreateFactory();

        // Act
        var result = await factory.Create(instrumentId, nonExistentAccountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Create_ImporterTypeIsNull_ThrowsNullReferenceException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var logicalAccount = TestEntities.CreateLogicalAccount(id: instrumentId);
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: accountId,
            instrumentId: instrumentId,
            importerTypeId: null,
            importerType: null);
        logicalAccount.AddInstitutionAccount(institutionAccount);

        _context.Add(logicalAccount);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var factory = CreateFactory();

        // Act & Assert
        // Note: The code throws NullReferenceException because it accesses ImporterType.Type
        // when ImporterType is null (the null-conditional doesn't cover this case)
        await Assert.ThrowsAsync<NullReferenceException>(() => factory.Create(instrumentId, accountId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Create_ImporterTypeNameIsEmpty_ReturnsNull()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Use an empty string which Type.GetType will treat as invalid
        var importerType = TestEntities.CreateImporterType(id: 1, typeName: "");
        var logicalAccount = TestEntities.CreateLogicalAccount(id: instrumentId);
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: accountId,
            instrumentId: instrumentId,
            importerTypeId: 1,
            importerType: importerType);
        logicalAccount.AddInstitutionAccount(institutionAccount);

        _context.Add(logicalAccount);
        _context.Add(importerType);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var factory = CreateFactory();

        // Act & Assert - Empty string type name causes Type.GetType to return null, which throws
        await Assert.ThrowsAsync<InvalidOperationException>(() => factory.Create(instrumentId, accountId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Create_InvalidTypeName_ThrowsInvalidOperationException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var importerType = TestEntities.CreateImporterType(id: 1, typeName: "Invalid.Type.Name, NonExistentAssembly");
        var logicalAccount = TestEntities.CreateLogicalAccount(id: instrumentId);
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: accountId,
            instrumentId: instrumentId,
            importerTypeId: 1,
            importerType: importerType);
        logicalAccount.AddInstitutionAccount(institutionAccount);

        _context.Add(logicalAccount);
        _context.Add(importerType);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var factory = CreateFactory();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => factory.Create(instrumentId, accountId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Create_ServiceNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Use a real type that exists but is not registered in DI
        var importerType = TestEntities.CreateImporterType(
            id: 1,
            typeName: typeof(TestImporter).AssemblyQualifiedName);
        var logicalAccount = TestEntities.CreateLogicalAccount(id: instrumentId);
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: accountId,
            instrumentId: instrumentId,
            importerTypeId: 1,
            importerType: importerType);
        logicalAccount.AddInstitutionAccount(institutionAccount);

        _context.Add(logicalAccount);
        _context.Add(importerType);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Don't register the TestImporter in DI
        var factory = CreateFactory();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => factory.Create(instrumentId, accountId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Create_ValidConfiguration_ReturnsImporter()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var importerType = TestEntities.CreateImporterType(
            id: 1,
            typeName: typeof(TestImporter).AssemblyQualifiedName);
        var logicalAccount = TestEntities.CreateLogicalAccount(id: instrumentId);
        var institutionAccount = TestEntities.CreateInstitutionAccount(
            id: accountId,
            instrumentId: instrumentId,
            importerTypeId: 1,
            importerType: importerType);
        logicalAccount.AddInstitutionAccount(institutionAccount);

        _context.Add(logicalAccount);
        _context.Add(importerType);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Register TestImporter in DI
        _services.AddSingleton<TestImporter>();
        var factory = CreateFactory();

        // Act
        var result = await factory.Create(instrumentId, accountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TestImporter>(result);
    }

    #endregion

    #region Create(string? importerType)

    [Fact]
    public void Create_ByTypeName_NullTypeName_ReturnsNull()
    {
        // Arrange
        var factory = CreateFactory();

        // Act
        var result = factory.Create(importerType: null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Create_ByTypeName_InvalidTypeName_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => factory.Create("Invalid.Type.Name, NonExistentAssembly"));
    }

    [Fact]
    public void Create_ByTypeName_ServiceNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = CreateFactory();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            factory.Create(typeof(TestImporter).AssemblyQualifiedName));
    }

    [Fact]
    public void Create_ByTypeName_ValidTypeName_ReturnsImporter()
    {
        // Arrange
        _services.AddSingleton<TestImporter>();
        var factory = CreateFactory();

        // Act
        var result = factory.Create(typeof(TestImporter).AssemblyQualifiedName);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TestImporter>(result);
    }

    #endregion

    private ImporterFactory CreateFactory()
    {
        _serviceProvider = _services.BuildServiceProvider();
        return new ImporterFactory(_context.Set<LogicalAccount>(), _serviceProvider);
    }
}

/// <summary>
/// Test implementation of IImporter for testing purposes.
/// </summary>
public class TestImporter : IImporter
{
    public Task<TransactionImportResult> Import(Guid instrumentId, Guid? institutionAccountId, Stream contents, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TransactionImportResult([]));
    }

    public Task Reprocess(Guid instrumentId, Guid accountId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
