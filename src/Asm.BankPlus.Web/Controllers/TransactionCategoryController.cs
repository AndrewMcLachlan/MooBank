using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [Route("api/transaction/category")]
    [ApiController]
    public class TransactionCategoryController : ControllerBase
    {
        private ITransactionCategoryRepository CategoryRepository { get; }

        public TransactionCategoryController(ITransactionCategoryRepository categoryRepository)
        {
            CategoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionCategory>>> Get()
        {
            return Ok(await CategoryRepository.GetTransactionCategories());
        }

        [HttpGet]
        [Route("id")]
        public async Task<ActionResult<TransactionCategory>> Get(int id)
        {
            return await CategoryRepository.GetTransactionCategory(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<TransactionCategory>> Create(TransactionCategory category)
        {
            return await CategoryRepository.CreateTransactionCategory(category);
        }

        [HttpPatch]
        [ValidateAntiForgeryToken]
        [Route("{id}")]
        public async Task<ActionResult<TransactionCategory>> Update(int id, [FromBody]TransactionCategory category)
        {
            return await CategoryRepository.UpdateTransactionCategory(category);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await CategoryRepository.DeleteTransactionCategory(id);
            return Ok();
        }
    }
}
