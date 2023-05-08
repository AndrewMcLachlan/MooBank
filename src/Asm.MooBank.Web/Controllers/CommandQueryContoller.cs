using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;

namespace Asm.MooBank.Web.Controllers;

[ApiController]
[Authorize]
public abstract class CommandQueryController : Controller
{
    protected IQueryDispatcher QueryDispatcher { get; private set; }
    protected ICommandDispatcher CommandDispatcher { get; private set; }

    public CommandQueryController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
    {
        QueryDispatcher = queryDispatcher;
        CommandDispatcher = commandDispatcher;
    }
}
