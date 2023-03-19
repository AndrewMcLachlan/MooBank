namespace Asm.MooBank.Web.Controllers;

[ApiController]
public class ImportController : ControllerBase
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    [Route("api/accounts/{accountid}/[controller]")]
    public async Task<ActionResult> Post(Guid accountId, IFormFile file, CancellationToken cancellationToken = default)
    {
        await _importService.Import(accountId, file, cancellationToken);

        return NoContent();
    }
}
