﻿namespace Asm.MooBank.Web.Controllers
{
    [Route("api/transaction/tags")]
    [ApiController]
    public class TransactionTagController : ControllerBase
    {
        private readonly ITransactionTagService _tagService;

        public TransactionTagController(ITransactionTagService categoryService)
        {
            _tagService = categoryService;
        }

        [HttpGet]
        public Task<IEnumerable<TransactionTag>> Get(CancellationToken cancellationToken = default) => _tagService.GetAll(cancellationToken);

        [HttpGet("{id}")]
        public async Task<TransactionTag> Get(int id, CancellationToken cancellationToken = default)
        {
            return await _tagService.Get(id, cancellationToken);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionTag>> Create(TransactionTag tag)
        {
            var newTag = await _tagService.Create(tag);

            return Created($"api/transaction/tags/{newTag.Id}", newTag);
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<TransactionTag>> CreateByName(string name, [FromBody]TransactionTag[] tags)
        {
            var tag = await _tagService.Create(new TransactionTag { Name = Uri.UnescapeDataString(name), Tags = tags });

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
