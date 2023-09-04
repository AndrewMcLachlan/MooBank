using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
//using Asm.MooBank.Commands.Institution;
using Asm.MooBank.Queries.Institution;
using Asm.MooBank.Security;

namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = Policies.Admin)]
public class InstitutionsController : CommandQueryController
{
    public InstitutionsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : base(queryDispatcher, commandDispatcher)
    {

    }

    [HttpGet]
    public Task<IEnumerable<MooBank.Models.Institution>> GetAll() =>
        QueryDispatcher.Dispatch(new GetAll());


    /*[HttpGet("{id}")]
    public Task<Institution> Get(Guid id) =>
        QueryDispatcher.Dispatch(new Get(id));

    [HttpPost]
    public async Task<IActionResult> Create(Institution institution)
    {
        var createdGroup = await CommandDispatcher.Dispatch(new Create(institution));

        return CreatedAtAction(nameof(Get), new { id = createdGroup.Id }, createdGroup);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Institution institution)
    {
        if (institution.Id != id) return BadRequest();

        return Ok(await CommandDispatcher.Dispatch(new Update(institution)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await CommandDispatcher.Dispatch(new Delete(id));

        return NoContent();
    }*/
}
