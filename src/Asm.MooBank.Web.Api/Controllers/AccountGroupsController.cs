﻿using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.AccountGroup;
using Asm.MooBank.Queries.AccountGroup;

namespace Asm.MooBank.Web.Controllers;

[Route("api/account-groups-old")]
public class AccountGroupsController : CommandQueryController
{
    public AccountGroupsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {

    }

    [HttpGet]
    public Task<IEnumerable<AccountGroup>> GetAll() =>
        QueryDispatcher.Dispatch(new GetAll());


    [HttpGet("{id}")]
    public Task<AccountGroup> Get(Guid id) =>
        QueryDispatcher.Dispatch(new Get(id));

    [HttpPost]
    public async Task<IActionResult> Create(AccountGroup accountGroup)
    {
        var createdGroup = await CommandDispatcher.Dispatch(new Create(accountGroup));

        return CreatedAtAction(nameof(Get), new { id = createdGroup.Id }, createdGroup);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AccountGroup accountGroup)
    {
        if (accountGroup.Id != id) return BadRequest();

        return Ok(await CommandDispatcher.Dispatch(new Update(accountGroup)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await CommandDispatcher.Dispatch(new Delete(id));

        return NoContent();
    }
}