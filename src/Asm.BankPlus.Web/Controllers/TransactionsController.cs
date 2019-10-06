using System;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Web.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPut]
        //[ValidateAntiForgeryToken]
        [Route("{id}/tag/{tagId}")]
        public async Task<ActionResult<Transaction>> Add(Guid id, int tagId)
        {
            return await TransactionRepository.AddTransactionTag(id, tagId);
        }

        [HttpDelete]
        //[ValidateAntiForgeryToken]
        [Route("{id}/tag/{tagId}")]
        public async Task<ActionResult<Transaction>> RemoveTag(Guid id, int tagId)
        {
            return await TransactionRepository.RemoveTransactionTag(id, tagId);
        }
    }
}