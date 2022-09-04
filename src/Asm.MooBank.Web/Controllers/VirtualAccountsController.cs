namespace Asm.MooBank.Web.Controllers;

[Route("api/accounts/{accountId}/virtual")]
[ApiController]
[Authorize]
public class VirtualAccountsController : ControllerBase
{
    private readonly IVirtualAccountRepository _virtualAccountRepository;

    public VirtualAccountsController(IVirtualAccountRepository virtualAccountRepository)
    {
        _virtualAccountRepository = virtualAccountRepository;
    }

    [HttpGet("{virtualAccountId}")]
    public Task<VirtualAccount> Get(Guid accountId, Guid virtualAccountId)
    {
        return _virtualAccountRepository.Get(accountId, virtualAccountId);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid accountId, VirtualAccount account)
    {
        var model = await _virtualAccountRepository.Create(accountId, account);

        return CreatedAtAction("Get", new { accountId = accountId, virtualAccountId = model.Id }, model);
    }

    [HttpPatch("{virtualAccountId}")]
    public async Task<IActionResult> Update(Guid accountId, Guid virtualAccountId, VirtualAccount account)
    {
        if (virtualAccountId != account.Id) return BadRequest(ModelState);

        return Ok(await _virtualAccountRepository.Update(accountId, account));
    }

    [HttpPatch("{virtualAccountId}/balance")]
    public async Task<IActionResult> UpdateBalance(Guid accountId, Guid virtualAccountId, UpdateVirtualBalanceModel balance)
    {
        return Ok(await _virtualAccountRepository.UpdateBalance(accountId, virtualAccountId, balance.Balance));
    }

    [HttpDelete("{virtualAccountId}")]
    public async Task<IActionResult> Delete(Guid accountId, Guid virtualAccountId)
    {
        await _virtualAccountRepository.Delete(accountId, virtualAccountId);

        return Ok();
    }
}
