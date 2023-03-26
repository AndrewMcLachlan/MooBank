using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Commands.AccountGroup;
using Asm.MooBank.Models.Queries.AccountGroup;

namespace Asm.MooBank.Web.Controllers;

[Route("api/account-groups")]
public class AccountGroupsController : CommandQueryController
{
    public AccountGroupsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {

    }

    [HttpGet]
    public Task<IEnumerable<AccountGroup>> GetAll() =>
        QueryDispatcher.Dispatch(new GetAccountGroups());


    [HttpGet("{id}")]
    public Task<AccountGroup> Get(Guid id) =>
        QueryDispatcher.Dispatch(new GetAccountGroup(id));

    [HttpPost]
    public async Task<IActionResult> Create(AccountGroup accountGroup)
    {
        var createdGroup = await CommandDispatcher.Dispatch(new CreateAccountGroup(accountGroup));

        return CreatedAtAction(nameof(Get), new { id = createdGroup.Id }, createdGroup);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AccountGroup accountGroup)
    {
        if (accountGroup.Id != id) return BadRequest();

        return Ok(await CommandDispatcher.Dispatch(new UpdateAccountGroup(accountGroup)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await CommandDispatcher.Dispatch(new DeleteAccountGroup(id));

        return NoContent();
    }
}
