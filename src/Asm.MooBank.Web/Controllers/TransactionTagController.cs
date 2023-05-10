using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.TransactionTags;
using Asm.MooBank.Queries.TransactionTags;

namespace Asm.MooBank.Web.Controllers
{
    [Route("api/transaction/tags")]
    [ApiController]
    public class TransactionTagController : CommandQueryController
    {
        private readonly ITransactionTagService _tagService;

        public TransactionTagController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ITransactionTagService categoryService) : base(queryDispatcher, commandDispatcher)
        {
            _tagService = categoryService;
        }

        [HttpGet]
        public Task<IEnumerable<TransactionTag>> Get(CancellationToken cancellationToken = default) => _tagService.GetAll(cancellationToken);

        [HttpGet("hierarchy")]
        public Task<TransactionTagHierarchy> GetHierarchy(CancellationToken cancellationToken = default) => QueryDispatcher.Dispatch(new GetTransactionTagsHierarchy(), cancellationToken);

        [HttpGet("{id}")]
        public async Task<TransactionTag> Get(int id, CancellationToken cancellationToken = default)
        {
            if (id == 0) return null!;
            return await _tagService.Get(id, cancellationToken);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionTag>> Create(TransactionTag tag, CancellationToken cancellationToken = default)
        {
            var newTag = await CommandDispatcher.Dispatch(new Create(tag), cancellationToken);

            return Created($"api/transaction/tags/{newTag.Id}", newTag);
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<TransactionTag>> CreateByName(string name, [FromBody]int[] tags, CancellationToken cancellationToken = default)
        {
            var tag = await CommandDispatcher.Dispatch(new CreateByName(name, tags), cancellationToken);

            return Created($"api/transaction/tags/{tag.Id}", tag);
        }

        [HttpPatch("{id}")]
        public Task<TransactionTag> Update(int id, [FromBody]TransactionTagModel tag, CancellationToken cancellationToken = default) =>
            CommandDispatcher.Dispatch(new Update(id, tag.Name, tag.ExcludeFromReporting, tag.ApplySmoothing), cancellationToken);

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _tagService.Delete(id);
            return NoContent();
        }

        [HttpPut("{id}/tags/{subId}")]
        public async Task<ActionResult<TransactionTag>> AddSubTag(int id, int subId, CancellationToken cancellationToken = default)
        {
            if (id == subId) return Conflict();

            var tag = await CommandDispatcher.Dispatch(new AddSubTag(id, subId), cancellationToken);

            return Created($"api/transaction/tags/{id}/tags/{subId}", tag);
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
