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
            return Ok(await TagRepository.GetTransactionTags());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionTag>> Get(int id)
        {
            return await TagRepository.GetTransactionTag(id);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionTag>> Create(TransactionTag category)
        {
            var tag = await TagRepository.CreateTransactionTag(category);

            return Created($"api/transaction/tags/{tag.Id}", tag);
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<TransactionTag>> CreateByName(string name)
        {
            var tag = await TagRepository.CreateTransactionTag(name);

            return Created($"api/transaction/tags/{tag.Id}", tag);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TransactionTag>> Update(int id, [FromBody]TransactionTagModel tag)
        {
            return await TagRepository.UpdateTransactionTag(id, tag.Name);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await TagRepository.DeleteTransactionTag(id);
            return NoContent();
        }
    }
}
