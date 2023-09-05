using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.Account;
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
    public Task<IEnumerable<InstitutionAccount>> Get(CancellationToken cancellationToken = default) => QueryDispatcher.Dispatch(new GetAll(), cancellationToken);

    [HttpGet("position")]
    public Task<AccountsList> GetFormatted(CancellationToken cancellationToken = default) => QueryDispatcher.Dispatch(new GetFormatted(), cancellationToken);

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
    public async Task<ActionResult<InstitutionAccount>> UpdateAccount(Guid accountId, InstitutionAccount model, CancellationToken cancellationToken = default)
    {
        if (model == null || model.Id != accountId) return BadRequest(ModelState);

        return Ok(await CommandDispatcher.Dispatch(new Update(model), cancellationToken));
    }

    [HttpPatch("{accountId}/balance")]
    public async Task<ActionResult<InstitutionAccount>> UpdateBalance(Guid accountId, UpdateBalanceModel model)
    {
        if (model == null || (model.CurrentBalance == null)) return BadRequest(ModelState);

        return Ok(await _accountService.SetBalance(accountId, model.CurrentBalance.Value));
    }
}
