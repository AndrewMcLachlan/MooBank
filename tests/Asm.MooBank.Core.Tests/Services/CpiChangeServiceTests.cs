using Asm.Domain;
using Asm.MooBank.Abs;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using DomainCpiChange = Asm.MooBank.Domain.Entities.ReferenceData.CpiChange;
using AbsCpiChange = Asm.MooBank.Abs.CpiChange;
using IReferenceDataRepository = Asm.MooBank.Domain.Entities.ReferenceData.IReferenceDataRepository;
using QuarterEntity = Asm.MooBank.Domain.Entities.QuarterEntity;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="CpiChangeService"/> service.
/// Tests cover updating CPI changes from external ABS data.
/// </summary>
public class CpiChangeServiceTests
{
    #region UpdateWithCpiChanges

    /// <summary>
    /// Given no existing CPI changes and new changes from ABS
    /// When UpdateWithCpiChanges is called
    /// Then all new changes should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateWithCpiChanges_NoExistingChanges_AddsAllNewChanges()
    {
        // Arrange
        var existingChanges = new List<DomainCpiChange>();
        var absChanges = new List<AbsCpiChange>
        {
            new() { Quarter = new Quarter(2024, 1), ChangePercent = 1.5m },
            new() { Quarter = new Quarter(2024, 2), ChangePercent = 2.0m },
        };

        var (service, repositoryMock, _) = CreateService(existingChanges, absChanges);

        // Act
        await service.UpdateWithCpiChanges();

        // Assert
        repositoryMock.Verify(r => r.AddCpiChange(It.IsAny<DomainCpiChange>()), Times.Exactly(2));
    }

    /// <summary>
    /// Given existing CPI changes that match ABS data
    /// When UpdateWithCpiChanges is called
    /// Then no new changes should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateWithCpiChanges_AllChangesExist_AddsNothing()
    {
        // Arrange
        var existingChanges = new List<DomainCpiChange>
        {
            new() { Quarter = new QuarterEntity(2024, 1), ChangePercent = 1.5m },
            new() { Quarter = new QuarterEntity(2024, 2), ChangePercent = 2.0m },
        };
        var absChanges = new List<AbsCpiChange>
        {
            new() { Quarter = new Quarter(2024, 1), ChangePercent = 1.5m },
            new() { Quarter = new Quarter(2024, 2), ChangePercent = 2.0m },
        };

        var (service, repositoryMock, _) = CreateService(existingChanges, absChanges);

        // Act
        await service.UpdateWithCpiChanges();

        // Assert
        repositoryMock.Verify(r => r.AddCpiChange(It.IsAny<DomainCpiChange>()), Times.Never);
    }

    /// <summary>
    /// Given some existing CPI changes and new changes from ABS
    /// When UpdateWithCpiChanges is called
    /// Then only new changes should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateWithCpiChanges_SomeChangesExist_AddsOnlyNewChanges()
    {
        // Arrange
        var existingChanges = new List<DomainCpiChange>
        {
            new() { Quarter = new QuarterEntity(2024, 1), ChangePercent = 1.5m },
        };
        var absChanges = new List<AbsCpiChange>
        {
            new() { Quarter = new Quarter(2024, 1), ChangePercent = 1.5m }, // Exists
            new() { Quarter = new Quarter(2024, 2), ChangePercent = 2.0m }, // New
            new() { Quarter = new Quarter(2024, 3), ChangePercent = 1.8m }, // New
        };

        var (service, repositoryMock, _) = CreateService(existingChanges, absChanges);

        // Act
        await service.UpdateWithCpiChanges();

        // Assert - Only 2 new changes added
        repositoryMock.Verify(r => r.AddCpiChange(It.IsAny<DomainCpiChange>()), Times.Exactly(2));
    }

    /// <summary>
    /// Given CPI changes are added
    /// When UpdateWithCpiChanges completes
    /// Then SaveChangesAsync should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateWithCpiChanges_Always_SavesChanges()
    {
        // Arrange
        var (service, _, unitOfWorkMock) = CreateService([], []);

        // Act
        await service.UpdateWithCpiChanges();

        // Assert
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    /// <summary>
    /// Given ABS returns no changes
    /// When UpdateWithCpiChanges is called
    /// Then nothing should be added but SaveChanges is still called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateWithCpiChanges_AbsReturnsEmpty_AddsNothing()
    {
        // Arrange
        var existingChanges = new List<DomainCpiChange>
        {
            new() { Quarter = new QuarterEntity(2024, 1), ChangePercent = 1.5m },
        };
        var absChanges = new List<AbsCpiChange>();

        var (service, repositoryMock, unitOfWorkMock) = CreateService(existingChanges, absChanges);

        // Act
        await service.UpdateWithCpiChanges();

        // Assert
        repositoryMock.Verify(r => r.AddCpiChange(It.IsAny<DomainCpiChange>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    private static (ICpiChangeService service, Mock<IReferenceDataRepository> repositoryMock, Mock<IUnitOfWork> unitOfWorkMock) CreateService(
        List<DomainCpiChange> existingChanges,
        List<AbsCpiChange> absChanges)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var absClientMock = new Mock<IAbsClient>();
        var repositoryMock = new Mock<IReferenceDataRepository>();

        repositoryMock.Setup(r => r.GetCpiChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingChanges);

        absClientMock.Setup(c => c.GetCpiChanges(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(absChanges);

        var service = new CpiChangeService(unitOfWorkMock.Object, absClientMock.Object, repositoryMock.Object);

        return (service, repositoryMock, unitOfWorkMock);
    }
}
