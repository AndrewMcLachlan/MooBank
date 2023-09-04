using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Queries.VirtualAccount;

namespace Asm.MooBank.Web.Controllers;

[Route("api/accounts/{accountId}/virtual")]
[ApiController]
[Authorize]
public class VirtualAccountsController : CommandQueryController
{
    private readonly IVirtualAccountService _virtualAccountService;

    public VirtualAccountsController(IVirtualAccountService virtualAccountService, IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {
        _virtualAccountService = virtualAccountService;
    }

    [HttpGet]
    public Task<IEnumerable<VirtualAccount>> Get(Guid accountId, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new GetForAccount(accountId), cancellationToken);

    [HttpGet("{virtualAccountId}")]
    public Task<VirtualAccount> Get(Guid accountId, Guid virtualAccountId)
    {
        return _virtualAccountService.Get(accountId, virtualAccountId);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid accountId, VirtualAccount account)
    {
        var model = await _virtualAccountService.Create(accountId, account);

        return CreatedAtAction("Get", new { accountId, virtualAccountId = model.Id }, model);
    }

    [HttpPatch("{virtualAccountId}")]
    public async Task<IActionResult> Update(Guid accountId, Guid virtualAccountId, VirtualAccount account)
    {
        if (virtualAccountId != account.Id) return BadRequest(ModelState);

        return Ok(await _virtualAccountService.Update(accountId, account));
    }

    [HttpPatch("{virtualAccountId}/balance")]
    public async Task<IActionResult> UpdateBalance(Guid accountId, Guid virtualAccountId, UpdateVirtualBalanceModel balance)
    {
        return Ok(await _virtualAccountService.UpdateBalance(accountId, virtualAccountId, balance.Balance));
    }

    [HttpDelete("{virtualAccountId}")]
    public async Task<IActionResult> Delete(Guid accountId, Guid virtualAccountId)
    {
        await _virtualAccountService.Delete(accountId, virtualAccountId);

        return Ok();
    }
}
