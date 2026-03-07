using Asm.Domain;
using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Importers;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="ReprocessTransactionsService"/> class.
/// Tests verify reprocessing orchestration, error handling, and logging.
/// </summary>
public class ReprocessTransactionsServiceTests
{
    private readonly Mock<IImporterFactory> _importerFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IImporter> _importerMock;
    private readonly ILogger<ReprocessTransactionsService> _logger;

    public ReprocessTransactionsServiceTests()
    {
        _importerFactoryMock = new Mock<IImporterFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _importerMock = new Mock<IImporter>();
        _logger = NullLoggerFactory.Instance.CreateLogger<ReprocessTransactionsService>();

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #region Successful Reprocessing

    /// <summary>
    /// Given a valid work item and available importer
    /// When Reprocess is called
    /// Then the importer's Reprocess method should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WithValidWorkItem_CallsImporterReprocess()
    {
        // Arrange
        var workItem = CreateWorkItem();
        SetupImporterFactory(workItem, _importerMock.Object);
        var service = CreateService();

        // Act
        await service.Reprocess(workItem, TestContext.Current.CancellationToken);

        // Assert
        _importerMock.Verify(i => i.Reprocess(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a valid work item and available importer
    /// When Reprocess is called
    /// Then changes should be saved to the database
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WithValidWorkItem_SavesChanges()
    {
        // Arrange
        var workItem = CreateWorkItem();
        SetupImporterFactory(workItem, _importerMock.Object);
        var service = CreateService();

        // Act
        await service.Reprocess(workItem, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a valid work item
    /// When Reprocess is called successfully
    /// Then operations should complete in correct order
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WithValidWorkItem_CallsOperationsInOrder()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var callOrder = new List<string>();

        _importerFactoryMock
            .Setup(f => f.Create(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callOrder.Add("Factory");
                return _importerMock.Object;
            });

        _importerMock
            .Setup(i => i.Reprocess(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Reprocess"))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Save"))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        await service.Reprocess(workItem, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["Factory", "Reprocess", "Save"], callOrder);
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Given a work item for an account without importer support
    /// When Reprocess is called
    /// Then InvalidOperationException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WhenImporterNotSupported_ThrowsInvalidOperationException()
    {
        // Arrange
        var workItem = CreateWorkItem();
        _importerFactoryMock
            .Setup(f => f.Create(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IImporter?)null);

        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.Reprocess(workItem, TestContext.Current.CancellationToken));
        Assert.Contains(workItem.AccountId.ToString(), exception.Message);
    }

    /// <summary>
    /// Given an importer that throws during reprocessing
    /// When Reprocess is called
    /// Then the exception should be rethrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WhenImporterThrows_RethrowsException()
    {
        // Arrange
        var workItem = CreateWorkItem();
        SetupImporterFactory(workItem, _importerMock.Object);

        _importerMock
            .Setup(i => i.Reprocess(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Importer error"));

        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.Reprocess(workItem, TestContext.Current.CancellationToken));
        Assert.Equal("Importer error", exception.Message);
    }

    /// <summary>
    /// Given an importer that throws during reprocessing
    /// When Reprocess is called
    /// Then save should not be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WhenImporterThrows_DoesNotSave()
    {
        // Arrange
        var workItem = CreateWorkItem();
        SetupImporterFactory(workItem, _importerMock.Object);

        _importerMock
            .Setup(i => i.Reprocess(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Importer error"));

        var service = CreateService();

        // Act
        try
        {
            await service.Reprocess(workItem, TestContext.Current.CancellationToken);
        }
        catch
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a unit of work that throws during save
    /// When Reprocess is called
    /// Then the exception should be rethrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WhenSaveThrows_RethrowsException()
    {
        // Arrange
        var workItem = CreateWorkItem();
        SetupImporterFactory(workItem, _importerMock.Object);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.Reprocess(workItem, TestContext.Current.CancellationToken));
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region Cancellation

    /// <summary>
    /// Given a cancellation token
    /// When Reprocess is called
    /// Then the token should be passed to the importer factory
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WithCancellationToken_PassesToFactory()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var cts = new CancellationTokenSource();
        SetupImporterFactory(workItem, _importerMock.Object);
        var service = CreateService();

        // Act
        await service.Reprocess(workItem, cts.Token);

        // Assert
        _importerFactoryMock.Verify(
            f => f.Create(workItem.InstrumentId, workItem.AccountId, cts.Token),
            Times.Once);
    }

    /// <summary>
    /// Given a cancellation token
    /// When Reprocess is called
    /// Then the token should be passed to the importer
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Reprocess_WithCancellationToken_PassesToImporter()
    {
        // Arrange
        var workItem = CreateWorkItem();
        var cts = new CancellationTokenSource();
        SetupImporterFactory(workItem, _importerMock.Object);
        var service = CreateService();

        // Act
        await service.Reprocess(workItem, cts.Token);

        // Assert
        _importerMock.Verify(
            i => i.Reprocess(workItem.InstrumentId, workItem.AccountId, cts.Token),
            Times.Once);
    }

    #endregion

    #region Helpers

    private IReprocessTransactionsService CreateService() =>
        new ReprocessTransactionsService(
            _importerFactoryMock.Object,
            _unitOfWorkMock.Object,
            _logger);

    private void SetupImporterFactory(ReprocessWorkItem workItem, IImporter importer)
    {
        _importerFactoryMock
            .Setup(f => f.Create(workItem.InstrumentId, workItem.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(importer);
    }

    private static ReprocessWorkItem CreateWorkItem()
    {
        return new ReprocessWorkItem(TestModels.AccountId, Guid.NewGuid());
    }

    #endregion
}
