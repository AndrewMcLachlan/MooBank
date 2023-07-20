using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.Import;

namespace Asm.MooBank.Web.Controllers;

[ApiController]
public class ImportController : CommandQueryController
{
    private readonly IImportService _importService;

    public ImportController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IImportService importService) : base(queryDispatcher, commandDispatcher)
    {
        _importService = importService;
    }

    [HttpPost("api/accounts/{accountid}/[controller]")]
    public async Task<ActionResult> Import(Guid accountId, IFormFile file, CancellationToken cancellationToken = default)
    {
        using Stream stream = file.OpenReadStream();

        await _importService.Import(accountId, stream, cancellationToken);

        return NoContent();
    }

    [HttpPost("api/accounts/{accountid}/[controller]/reprocess")]
    public async Task<ActionResult> Reprocess(Guid accountId, CancellationToken cancellationToken = default)
    {
        await CommandDispatcher.Dispatch(new Reprocess(accountId), cancellationToken);

        return NoContent();
    }
}
