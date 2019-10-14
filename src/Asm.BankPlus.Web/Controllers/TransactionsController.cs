using System;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    // [ValidateAntiForgeryToken]
    public class TransactionsController : ControllerBase
    {
        private ITransactionRepository TransactionRepository { get; }
        private IAntiforgery Antiforgery { get; }

        public TransactionsController(ITransactionRepository transactionRepository, IAntiforgery antiforgery)
        {
            TransactionRepository = transactionRepository;
            Antiforgery = antiforgery;
        }

        [HttpPut("{id}/tag/{tagId}")]
        public async Task<ActionResult<Transaction>> Add(Guid id, int tagId)
        {
            return Created($"api/transactions/{id}/tag/{tagId}",
                await TransactionRepository.AddTransactionTag(id, tagId));
        }

        [HttpDelete("{id}/tag/{tagId}")]
        public async Task<ActionResult<Transaction>> RemoveTag(Guid id, int tagId)
        {
            return await TransactionRepository.RemoveTransactionTag(id, tagId);
        }
    }
}