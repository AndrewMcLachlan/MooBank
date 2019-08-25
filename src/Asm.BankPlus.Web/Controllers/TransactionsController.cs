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
    [Route("api/accounts/{accountId}/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpGet]
        public async Task<ActionResult<TransactionsModel>> Get(Guid accountId)
        {
            return new ActionResult<TransactionsModel>(new TransactionsModel
            {
                Transactions = await TransactionRepository.GetTransactions(accountId)
            });
        }

        [HttpPut]
        [HttpPatch]
        [ValidateAntiForgeryToken]
        [Route("{id}/{categoryId}")]
        public async Task<ActionResult<Transaction>> SetCategory(Guid accountId, Guid id, int categoryId)
        {
            return await TransactionRepository.SetTransactionCategory(id, categoryId);
        }
    }
}