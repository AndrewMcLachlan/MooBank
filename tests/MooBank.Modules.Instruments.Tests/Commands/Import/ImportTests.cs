#nullable enable
using Asm.MooBank.Modules.Instruments.Tests.Support;
using ImportCommand = Asm.MooBank.Modules.Instruments.Commands.Import.Import;
using ImportHandler = Asm.MooBank.Modules.Instruments.Commands.Import.ImportHandler;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Import;

[Trait("Category", "Unit")]
public class ImportTests
{
    private readonly TestMocks _mocks;

    public ImportTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_QueuesImport()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var fileContent = "test,data,content"u8.ToArray();
        using var stream = new MemoryStream(fileContent);

        var handler = new ImportHandler(_mocks.ImportQueueMock.Object, _mocks.User);
        var command = new ImportCommand(instrumentId, accountId, stream);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ImportQueueMock.Verify(
            q => q.QueueImport(instrumentId, accountId, _mocks.User, It.IsAny<byte[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesFileDataToQueue()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var fileContent = "test,data,content"u8.ToArray();
        using var stream = new MemoryStream(fileContent);
        byte[]? capturedData = null;

        _mocks.ImportQueueMock
            .Setup(q => q.QueueImport(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MooBank.Models.User>(), It.IsAny<byte[]>()))
            .Callback<Guid, Guid, MooBank.Models.User, byte[]>((_, _, _, data) => capturedData = data);

        var handler = new ImportHandler(_mocks.ImportQueueMock.Object, _mocks.User);
        var command = new ImportCommand(instrumentId, accountId, stream);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedData);
        Assert.Equal(fileContent, capturedData);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCorrectInstrumentId()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        using var stream = new MemoryStream("data"u8.ToArray());
        Guid? capturedInstrumentId = null;

        _mocks.ImportQueueMock
            .Setup(q => q.QueueImport(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MooBank.Models.User>(), It.IsAny<byte[]>()))
            .Callback<Guid, Guid, MooBank.Models.User, byte[]>((iid, _, _, _) => capturedInstrumentId = iid);

        var handler = new ImportHandler(_mocks.ImportQueueMock.Object, _mocks.User);
        var command = new ImportCommand(instrumentId, accountId, stream);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(instrumentId, capturedInstrumentId);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCorrectAccountId()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        using var stream = new MemoryStream("data"u8.ToArray());
        Guid? capturedAccountId = null;

        _mocks.ImportQueueMock
            .Setup(q => q.QueueImport(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MooBank.Models.User>(), It.IsAny<byte[]>()))
            .Callback<Guid, Guid, MooBank.Models.User, byte[]>((_, aid, _, _) => capturedAccountId = aid);

        var handler = new ImportHandler(_mocks.ImportQueueMock.Object, _mocks.User);
        var command = new ImportCommand(instrumentId, accountId, stream);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(accountId, capturedAccountId);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCurrentUser()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        using var stream = new MemoryStream("data"u8.ToArray());
        MooBank.Models.User? capturedUser = null;

        _mocks.ImportQueueMock
            .Setup(q => q.QueueImport(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MooBank.Models.User>(), It.IsAny<byte[]>()))
            .Callback<Guid, Guid, MooBank.Models.User, byte[]>((_, _, user, _) => capturedUser = user);

        var handler = new ImportHandler(_mocks.ImportQueueMock.Object, _mocks.User);
        var command = new ImportCommand(instrumentId, accountId, stream);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(_mocks.User, capturedUser);
    }

    [Fact]
    public async Task Handle_EmptyStream_QueuesEmptyData()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        using var stream = new MemoryStream();
        byte[]? capturedData = null;

        _mocks.ImportQueueMock
            .Setup(q => q.QueueImport(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MooBank.Models.User>(), It.IsAny<byte[]>()))
            .Callback<Guid, Guid, MooBank.Models.User, byte[]>((_, _, _, data) => capturedData = data);

        var handler = new ImportHandler(_mocks.ImportQueueMock.Object, _mocks.User);
        var command = new ImportCommand(instrumentId, accountId, stream);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedData);
        Assert.Empty(capturedData);
    }

    [Fact]
    public async Task Handle_LargeStream_QueuesAllData()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var largeContent = new byte[10000];
        new Random(42).NextBytes(largeContent);
        using var stream = new MemoryStream(largeContent);
        byte[]? capturedData = null;

        _mocks.ImportQueueMock
            .Setup(q => q.QueueImport(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MooBank.Models.User>(), It.IsAny<byte[]>()))
            .Callback<Guid, Guid, MooBank.Models.User, byte[]>((_, _, _, data) => capturedData = data);

        var handler = new ImportHandler(_mocks.ImportQueueMock.Object, _mocks.User);
        var command = new ImportCommand(instrumentId, accountId, stream);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedData);
        Assert.Equal(largeContent.Length, capturedData.Length);
        Assert.Equal(largeContent, capturedData);
    }
}
