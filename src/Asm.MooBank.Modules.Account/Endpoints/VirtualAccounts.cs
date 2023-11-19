using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Account.Commands.VirtualAccount;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Modules.Account.Queries.VirtualAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Account.Endpoints;
internal class VirtualAccounts : EndpointGroupBase
{
    public override string Name => "Virtual Accounts";

    public override string Path => "/accounts/{accountId}/virtual";

    public override string Tags => Name;

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetForAccount, IEnumerable<VirtualAccount>>("/")
            .WithNames("Get Virtual Accounts");

        builder.MapQuery<Get, VirtualAccount>("/{virtualAccountId}")
            .WithNames("Get Virtual Account");

        builder.MapPostCreate<Create, VirtualAccount>("/", "Get Virtual Account".ToMachine(), a => new { VirtualAccountId = a.Id }, CommandBinding.Parameters)
            .WithNames("Create Virtual Account");

        builder.MapPatchCommand<Update, VirtualAccount>("/{virtualAccountId}", CommandBinding.None)
            .WithNames("Update Virtual Account");

        builder.MapPatchCommand<UpdateBalance, VirtualAccount>("/{virtualAccountId}/balance")
            .WithNames("Set Virtual Account Balance");

        builder.MapDelete<Delete>("/{virtualAccountId}")
            .WithNames("Delete Virtual Account");

        /*
         *
         *     [HttpGet]
    public Task<IEnumerable<VirtualAccount>> Get(Guid accountId, CancellationToken cancellationToken = default) =>
        QueryDispatcher.Dispatch(new GetForAccount(accountId), cancellationToken).AsTask();

    [HttpGet("{virtualAccountId}")]
    public Task<VirtualAccount> Get(Guid accountId, Guid virtualAccountId, CancellationToken cancellationToken = default)
    {
        return QueryDispatcher.Dispatch(new Get(accountId, virtualAccountId), cancellationToken).AsTask();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid accountId, VirtualAccount account, CancellationToken cancellationToken = default)
    {
        var model = await CommandDispatcher.Dispatch(new Create(accountId, account), cancellationToken);

        return CreatedAtAction("Get", new { accountId, virtualAccountId = model.Id }, model);
    }

    [HttpPatch("{virtualAccountId}")]
    public async Task<IActionResult> Update(Guid accountId, Guid virtualAccountId, VirtualAccount account, CancellationToken cancellationToken = default)
    {
        if (virtualAccountId != account.Id) return BadRequest(ModelState);

        return Ok(await CommandDispatcher.Dispatch(new Update(accountId, virtualAccountId, account.Name, account.Description, account.CurrentBalance), cancellationToken));
    }

    [HttpPatch("{virtualAccountId}/balance")]
    public async Task<IActionResult> UpdateBalance(Guid accountId, Guid virtualAccountId, UpdateVirtualBalanceModel balance, CancellationToken cancellationToken = default)
    {
        return Ok(await CommandDispatcher.Dispatch(new UpdateBalance(accountId, virtualAccountId, balance.Balance), cancellationToken));
    }

    [HttpDelete("{virtualAccountId}")]
    public async Task<IActionResult> Delete(Guid accountId, Guid virtualAccountId, CancellationToken cancellationToken = default)
    {
        await CommandDispatcher.Dispatch(new Delete(accountId, virtualAccountId), cancellationToken);

        return Ok();
    }*/
    }
}
