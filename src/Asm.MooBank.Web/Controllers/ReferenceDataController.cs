namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReferenceDataController : ControllerBase
{
    private readonly IReferenceDataService _service;

    public ReferenceDataController(IReferenceDataService service)
    {
        _service = service;
    }

    [HttpGet("importertypes")]
    public async Task<ActionResult<IEnumerable<ImporterType>>> GetImporterTypes()
    {
        return Ok(await _service.GetImporterTypes());
    }
}
