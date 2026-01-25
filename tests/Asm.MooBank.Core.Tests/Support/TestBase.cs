namespace Asm.MooBank.Core.Tests.Support;

/// <summary>
/// Base class for tests that need access to common test infrastructure.
/// </summary>
public abstract class TestBase
{
    protected Mocks Mocks { get; } = new();
    protected TestEntities Entities { get; } = new();
}
