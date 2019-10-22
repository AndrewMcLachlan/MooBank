using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [Route("api/transaction/tags")]
    [ApiController]
    public class TransactionTagController : ControllerBase
    {
        private ITransactionTagRepository TagRepository { get; }

        public TransactionTagController(ITransactionTagRepository categoryRepository)
        {
            TagRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionTag>>> Get()
        {
            return Ok(await TagRepository.Get());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionTag>> Get(int id)
        {
            return await TagRepository.Get(id);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionTag>> Create(TransactionTag tag)
        {
            var newTag = await TagRepository.Create(tag);

            return Created($"api/transaction/tags/{newTag.Id}", newTag);
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<TransactionTag>> CreateByName(string name, [FromBody]TransactionTag[] tags)
        {
            var tag = await TagRepository.Create(new TransactionTag { Name = name, Tags = tags });

            return Created($"api/transaction/tags/{tag.Id}", tag);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TransactionTag>> Update(int id, [FromBody]TransactionTagModel tag)
        {
            return await TagRepository.Update(id, tag.Name);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await TagRepository.Delete(id);
            return NoContent();
        }

        [HttpPut("{id}/tags/{subId}")]
        public async Task<ActionResult<TransactionTag>> AddSubTag(int id, int subId)
        {
            if (id == subId) return Conflict();

            var tag = await TagRepository.AddSubTag(id, subId);

            return Created($"api/transaction/tags/{id}/tags/{subId}", tag);
        }

        [HttpDelete("{id}/tags/{subId}")]
        public async Task<ActionResult> RemoveSubTag(int id, int subId)
        {
            if (id == subId) return Conflict();

            await TagRepository.RemoveSubTag(id, subId);
            return NoContent();
        }
    }
}
