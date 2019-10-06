using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
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

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<TransactionTag>> Get(int id)
        {
            return await TagRepository.GetTransactionTag(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<TransactionTag>> Create(TransactionTag category)
        {
            return await TagRepository.CreateTransactionTag(category);
        }

        [HttpPut("{name}")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult<TransactionTag>> CreateByName(string name)
        {
            return await TagRepository.CreateTransactionTag(name);
        }

        [HttpPatch]
        [ValidateAntiForgeryToken]
        [Route("{id}")]
        public async Task<ActionResult<TransactionTag>> Update(int id, [FromBody]TransactionTag category)
        {
            if (category.Id != id) return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Invaild body" });

            return await TagRepository.UpdateTransactionTag(category);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await TagRepository.DeleteTransactionTag(id);
            return Ok();
        }
    }
}
