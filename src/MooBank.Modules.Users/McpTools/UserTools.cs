using System.ComponentModel;
using Asm.MooBank.Modules.Users.Models;
using Asm.MooBank.Modules.Users.Queries;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Modules.Users.McpTools;

[McpServerToolType]
public class UserTools(IQueryDispatcher queryDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-me", ReadOnly = true, Title = "Get Current User")]
    [Description("Retrieves the currently authenticated user, including their preferred currency, family identifier, primary account, and the accounts they own or have shared access to. Use this to ground other queries in the user's context.")]
    public ValueTask<User> GetMe()
    {
        return queryDispatcher.Dispatch(new Get());
    }
}
