#nullable enable
using Asm.MooBank.Api.Tests.Authorization;
using Asm.MooBank.Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Api.Tests.Mcp;

/// <summary>
/// Tests that MCP tools are discovered from the module assemblies.
/// </summary>
/// <remarks>
/// Tools are registered by convention — <c>WithToolsFromAssemblies("Asm.MooBank.Modules")</c> scans
/// by assembly name prefix — so nothing in the compiler or a grep will tell you that a module's
/// tools failed to register. This asserts the composed host actually sees them.
///
/// Shares the authorization collection's factory rather than standing up its own: two
/// WebApplicationFactory instances starting concurrently race in HostFactoryResolver, and one of
/// them fails with "The entry point exited without ever building an IHost".
/// </remarks>
[Trait("Category", "Integration")]
[Collection(AuthorizationTestCollection.Name)]
public class McpToolDiscoveryTests(MooBankWebApplicationFactory factory)
{
    /// <summary>
    /// Given the composed application
    /// When the registered MCP tools are resolved
    /// Then every module's tools should be present
    /// </summary>
    [Theory]
    [InlineData("get-instruments")]
    [InlineData("get-transactions")]
    [InlineData("get-tags")]
    [InlineData("get-me")]
    [InlineData("get-bill-accounts")]
    [InlineData("import-bills")]
    public void RegisteredTools_IncludeEveryModulesTools(string toolName)
    {
        // Arrange / Act
        var toolNames = factory.Services.GetServices<McpServerTool>().Select(t => t.ProtocolTool.Name).ToList();

        // Assert
        Assert.Contains(toolName, toolNames);
    }

    /// <summary>
    /// Given the registered MCP tools
    /// When the bill import tool is inspected
    /// Then it should be advertised as a write operation
    /// </summary>
    /// <remarks>
    /// import-bills is the only tool that writes. Clients surface the read-only hint to the user,
    /// so getting it wrong understates what the tool does.
    /// </remarks>
    [Fact]
    public void ImportBillsTool_IsNotAdvertisedAsReadOnly()
    {
        // Arrange
        var tools = factory.Services.GetServices<McpServerTool>();

        // Act
        var importBills = Assert.Single(tools, t => t.ProtocolTool.Name == "import-bills");

        // Assert
        Assert.NotEqual(true, importBills.ProtocolTool.Annotations?.ReadOnlyHint);
    }
}
