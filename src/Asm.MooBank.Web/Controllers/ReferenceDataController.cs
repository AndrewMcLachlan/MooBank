namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReferenceDataController : ControllerBase
{
    private readonly IReferenceDataRepository _repository;

    public ReferenceDataController(IReferenceDataRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("importertypes")]
    public async Task<ActionResult<IEnumerable<ImporterType>>> GetImporterTypes()
    {
        return Ok(await _repository.GetImporterTypes());
    }
}
