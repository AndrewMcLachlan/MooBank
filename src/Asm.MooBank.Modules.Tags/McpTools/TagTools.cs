using System.ComponentModel;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Tags.Queries;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Modules.Tags.McpTools;

[McpServerToolType]
public class TagTools(IQueryDispatcher queryDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-tags", ReadOnly = true, Title = "Get Tags")]
    [Description("Retrieves a list of tags that are used to tag transaction")]
    public ValueTask<IEnumerable<Tag>> GetTags()
    {
        return queryDispatcher.Dispatch(new GetAll());
    }
}
