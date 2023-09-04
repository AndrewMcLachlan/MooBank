using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
//using Asm.MooBank.Commands.Family;
using Asm.MooBank.Queries.Family;
using Asm.MooBank.Security;

namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = Policies.Admin)]
public class FamiliesController : CommandQueryController
{
    public FamiliesController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {

    }

    [HttpGet]
    public Task<IEnumerable<Family>> GetAll() =>
        QueryDispatcher.Dispatch(new GetAll());


    /*[HttpGet("{id}")]
    public Task<Family> Get(Guid id) =>
        QueryDispatcher.Dispatch(new Get(id));

    [HttpPost]
    public async Task<IActionResult> Create(Family family)
    {
        var createdGroup = await CommandDispatcher.Dispatch(new Create(family));

        return CreatedAtAction(nameof(Get), new { id = createdGroup.Id }, createdGroup);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Family family)
    {
        if (family.Id != id) return BadRequest();

        return Ok(await CommandDispatcher.Dispatch(new Update(family)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await CommandDispatcher.Dispatch(new Delete(id));

        return NoContent();
    }*/
}
