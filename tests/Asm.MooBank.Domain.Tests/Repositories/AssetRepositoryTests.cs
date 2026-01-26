using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="AssetRepository"/> class.
/// Tests verify asset CRUD operations against an in-memory database.
/// </summary>
public class AssetRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;

    public AssetRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get

    /// <summary>
    /// Given assets exist
    /// When Get is called
    /// Then all assets should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingAssets_ReturnsAll()
    {
        // Arrange
        var asset1 = CreateAsset(Guid.NewGuid(), "House", 500000m);
        var asset2 = CreateAsset(Guid.NewGuid(), "Car", 25000m);

        _context.Set<Asset>().AddRange(asset1, asset2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new AssetRepository(_context);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given an asset exists
    /// When Get by id is called
    /// Then the asset should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingAsset_ReturnsAsset()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var asset = CreateAsset(assetId, "Test Asset", 100000m);
        _context.Set<Asset>().Add(asset);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new AssetRepository(_context);

        // Act
        var result = await repository.Get(assetId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Asset", result.Name);
        Assert.Equal(100000m, result.Value);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new asset
    /// When Add is called
    /// Then the asset should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewAsset_PersistsAsset()
    {
        // Arrange
        var repository = new AssetRepository(_context);
        var asset = CreateAsset(Guid.NewGuid(), "New Asset", 75000m);

        // Act
        repository.Add(asset);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedAsset = await _context.Set<Asset>().FirstOrDefaultAsync(a => a.Name == "New Asset", TestContext.Current.CancellationToken);
        Assert.NotNull(savedAsset);
        Assert.Equal(75000m, savedAsset.Value);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing asset
    /// When Update is called
    /// Then the asset should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingAsset_UpdatesAsset()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var asset = CreateAsset(assetId, "Original Name", 50000m);
        _context.Set<Asset>().Add(asset);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new AssetRepository(_context);

        // Act
        asset.Name = "Updated Name";
        asset.Value = 60000m;
        repository.Update(asset);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedAsset = await _context.Set<Asset>().FindAsync([assetId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedAsset!.Name);
        Assert.Equal(60000m, updatedAsset.Value);
    }

    #endregion

    private static Asset CreateAsset(Guid id, string name, decimal value) =>
        new(id)
        {
            Name = name,
            Currency = "AUD",
            Value = value,
            PurchasePrice = value * 0.8m,
        };
}
