using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Queries.Account;

namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountsController : CommandQueryController
{
    private readonly IAccountService _accountService;

    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IAccountService accountService, ILogger<AccountsController> logger) : base(queryDispatcher, commandDispatcher)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpGet]
    public Task<IEnumerable<InstitutionAccount>> Get(CancellationToken token = default) => _accountService.GetAccounts(token);

    [HttpGet("position")]
    public Task<AccountsList> GetFormatted(CancellationToken token = default) => _accountService.GetFormattedAccounts(token);

    [HttpGet("{id}")]
    public Task<InstitutionAccount> Get(Guid id, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new Get(id), cancellationToken);

    [HttpPost]
    public async Task<ActionResult<InstitutionAccount>> Create(NewAccountModel model)
    {
        var newAccount = await _accountService.Create(model.Account);

        return CreatedAtAction("Get", new { id = newAccount.Id }, newAccount);
    }

    [HttpPatch("{accountId}")]
    public async Task<ActionResult<InstitutionAccount>> UpdateAccount(Guid accountId, InstitutionAccount model)
    {
        if (model == null || model.Id != accountId) return BadRequest(ModelState);

        return Ok(await _accountService.Update(model));
    }

    [HttpPatch("{accountId}/balance")]
    public async Task<ActionResult<InstitutionAccount>> UpdateBalance(Guid accountId, UpdateBalanceModel model)
    {
        if (model == null || (model.CurrentBalance == null)) return BadRequest(ModelState);

        return Ok(await _accountService.SetBalance(accountId, model.CurrentBalance.Value));
    }
}
