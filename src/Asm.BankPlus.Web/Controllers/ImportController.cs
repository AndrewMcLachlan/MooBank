using Asm.BankPlus.Importers;

namespace Asm.BankPlus.Web.Controllers;

[ApiController]
public class ImportController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IImporterFactory _importerFactory;

    public ImportController(IAccountRepository accountRepository, IImporterFactory importerFactory)
    {
        _importerFactory = importerFactory;
        _accountRepository = accountRepository;
    }

    [Route("api/accounts/{accountid}/[controller]")]
    public async Task<ActionResult> Post(Guid accountId, IFormFile file)
    {
        var account = await _accountRepository.GetAccount(accountId);

        IImporter importer = _importerFactory.Create(accountId);

        await importer.Import(account, file.OpenReadStream());

        return NoContent();
    }
}
