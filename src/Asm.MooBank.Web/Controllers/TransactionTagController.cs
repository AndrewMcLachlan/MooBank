using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Queries.TransactionTags;

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
        public async Task<ActionResult<TransactionTag>> Create(TransactionTag tag)
        {
            var newTag = await _tagService.Create(tag);

            return Created($"api/transaction/tags/{newTag.Id}", newTag);
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<TransactionTag>> CreateByName(string name, [FromBody]int[] tags, CancellationToken cancellationToken = default)
        {
            var tag = await _tagService.Create(name, tags, cancellationToken);

            return Created($"api/transaction/tags/{tag.Id}", tag);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TransactionTag>> Update(int id, [FromBody]TransactionTagModel tag)
        {
            return await _tagService.Update(id, tag.Name);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _tagService.Delete(id);
            return NoContent();
        }

        [HttpPut("{id}/tags/{subId}")]
        public async Task<ActionResult<TransactionTag>> AddSubTag(int id, int subId)
        {
            if (id == subId) return Conflict();

            var tag = await _tagService.AddSubTag(id, subId);

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
