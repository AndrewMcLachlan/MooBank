using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.Tags;
using Asm.MooBank.Queries.Tags;

namespace Asm.MooBank.Web.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagController : CommandQueryController
    {
        private readonly ITransactionTagService _tagService;

        public TagController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ITransactionTagService categoryService) : base(queryDispatcher, commandDispatcher)
        {
            _tagService = categoryService;
        }

        [HttpGet]
        public Task<IEnumerable<Tag>> Get(CancellationToken cancellationToken = default) => QueryDispatcher.Dispatch(new GetAll(), cancellationToken);

        [HttpGet("hierarchy")]
        public Task<TransactionTagHierarchy> GetHierarchy(CancellationToken cancellationToken = default) => QueryDispatcher.Dispatch(new GetTagsHierarchy(), cancellationToken);

        [HttpGet("{id}")]
        public async Task<Tag> Get(int id, CancellationToken cancellationToken = default)
        {
            if (id == 0) return null!;
            return await _tagService.Get(id, cancellationToken);
        }

        [HttpPost]
        public async Task<ActionResult<Tag>> Create(Tag tag, CancellationToken cancellationToken = default)
        {
            var newTag = await CommandDispatcher.Dispatch(new Create(tag), cancellationToken);

            return Created($"api/tags/{newTag.Id}", newTag);
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<Tag>> CreateByName(string name, [FromBody]int[] tags, CancellationToken cancellationToken = default)
        {
            var tag = await CommandDispatcher.Dispatch(new CreateByName(name, tags), cancellationToken);

            return Created($"api/tags/{tag.Id}", tag);
        }

        [HttpPatch("{id}")]
        public Task<Tag> Update(int id, [FromBody]TransactionTagModel tag, CancellationToken cancellationToken = default) =>
            CommandDispatcher.Dispatch(new Update(id, tag.Name, tag.ExcludeFromReporting, tag.ApplySmoothing), cancellationToken);

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _tagService.Delete(id);
            return NoContent();
        }

        [HttpPut("{id}/tags/{subId}")]
        public async Task<ActionResult<Tag>> AddSubTag(int id, int subId, CancellationToken cancellationToken = default)
        {
            if (id == subId) return Conflict();

            var tag = await CommandDispatcher.Dispatch(new AddSubTag(id, subId), cancellationToken);

            return Created($"api/tags/{id}/tags/{subId}", tag);
        }

        [HttpDelete("{id}/tags/{subId}")]
        public async Task<ActionResult> RemoveSubTag(int id, int subId)
        {
            if (id == subId) return Conflict();

            await _tagService.RemoveSubTag(id, subId);
            return NoContent();
        }
    }
}
