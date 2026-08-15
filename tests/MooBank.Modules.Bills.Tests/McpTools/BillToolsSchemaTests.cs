#nullable enable
using System.Reflection;
using Asm.MooBank.Modules.Bills.McpTools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using Postie.Cqrs.Commands;
using Postie.Cqrs.Queries;

namespace Asm.MooBank.Modules.Bills.Tests.McpTools;

/// <summary>
/// Unit tests for the schemas the bill tools publish to an MCP client.
/// </summary>
/// <remarks>
/// A tool whose parameter is missing from its schema does not fail. The client cannot see the
/// parameter, so it sends nothing; the server supplies a default and the tool runs and reports
/// success over no data at all. Nothing throws and nothing is logged, so the only way this is
/// caught is by looking at the published schema, which is what these do.
/// </remarks>
[Trait("Category", "Unit")]
public class BillToolsSchemaTests
{
    /// <summary>
    /// Builds each tool the way the server does, which is the part that matters: with the
    /// dependency injection container available, so parameters it can satisfy are treated as
    /// dependencies rather than as arguments.
    /// </summary>
    private static IEnumerable<(MethodInfo Method, McpServerTool Tool)> Tools()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var target = new BillTools(Mock.Of<IQueryDispatcher>(), Mock.Of<ICommandDispatcher>());
        var options = new McpServerToolCreateOptions { Services = services };

        return typeof(BillTools)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() is not null)
            .Select(m => (m, McpServerTool.Create(m, target, options)));
    }

    private static string[] SchemaProperties(McpServerTool tool) =>
        tool.ProtocolTool.InputSchema.TryGetProperty("properties", out var properties)
            ? [.. properties.EnumerateObject().Select(p => p.Name)]
            : [];

    /// <summary>
    /// Given the bill tools as the server registers them
    /// When their schemas are published
    /// Then every parameter a caller has to supply should appear in them
    /// </summary>
    /// <remarks>
    /// The defect this pins down: import-bills took an IEnumerable of bills, which the container
    /// claims for every T and resolves to an empty array, so it was classed as a dependency and
    /// dropped from the schema. The tool published no parameters at all and imported nothing,
    /// whatever it was sent.
    ///
    /// Generic over the assembly rather than written against the one tool, because the same thing
    /// happens to any parameter the container will claim, and it never announces itself.
    /// </remarks>
    [Fact]
    public void EveryCallerSuppliedParameterAppearsInTheSchema()
    {
        foreach (var (method, tool) in Tools())
        {
            // A cancellation token is the framework's to provide, not the caller's.
            var expected = method.GetParameters()
                                 .Where(p => p.ParameterType != typeof(CancellationToken))
                                 .Select(p => p.Name!)
                                 .ToArray();

            var published = SchemaProperties(tool);

            Assert.Equal(expected.Order(), published.Order());
        }
    }

    /// <summary>
    /// Given the import tool
    /// When its schema is published
    /// Then the bills themselves should be described, not just the wrapper around them
    /// </summary>
    /// <remarks>
    /// The parameter appearing is not enough on its own -- a wrapper whose contents were dropped
    /// would still pass the test above while being just as useless to a caller.
    /// </remarks>
    [Fact]
    public void ImportBillsDescribesTheBills()
    {
        var (_, tool) = Tools().Single(t => t.Tool.ProtocolTool.Name == "import-bills");

        var schema = tool.ProtocolTool.InputSchema.GetRawText();

        Assert.Contains("\"bills\"", schema);
        Assert.Contains("accountName", schema);
        Assert.Contains("periods", schema);
    }
}
