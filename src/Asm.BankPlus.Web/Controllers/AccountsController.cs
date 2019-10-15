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
    [Authorize]
    // [ValidateAntiForgeryToken]
    public class AccountsController : ControllerBase
    {
        private IAccountRepository AccountRepository { get; }
        private ITransactionRepository TransactionRepository { get; }
        private ITransactionTagRuleRepository TransactionTagRuleRepository { get; }
        private IAntiforgery Antiforgery { get; }

        public AccountsController(IAccountRepository accountRepository, ITransactionRepository transactionRepository, ITransactionTagRuleRepository transactionTagRuleRepository, IAntiforgery antiforgery)
        {
            AccountRepository = accountRepository;
            TransactionRepository = transactionRepository;
            Antiforgery = antiforgery;
            TransactionTagRuleRepository = transactionTagRuleRepository;
        }

        [HttpGet]
        public async Task<ActionResult<AccountsModel>> Get()
        {
            return new ActionResult<AccountsModel>(new AccountsModel
            {
                Accounts = await AccountRepository.GetAccounts()
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> Get(Guid id)
        {
            return new ActionResult<Account>(await AccountRepository.GetAccount(id));
        }

        [HttpGet("{accountId}/transactions/{pageNumber?}")]
        public async Task<ActionResult<TransactionsModel>> Get(Guid accountId, int? pageNumber = 1)
        {
            return new ActionResult<TransactionsModel>(new TransactionsModel
            {
                Transactions = await TransactionRepository.GetTransactions(accountId)
            });
        }

        [HttpGet("{accountId}/transaction/tag/rules")]
        public async Task<ActionResult<TransactionTagRulesModel>> GetTagRules(Guid accountId)
        {
            return new ActionResult<TransactionTagRulesModel>(new TransactionTagRulesModel
            {
                Rules = await TransactionTagRuleRepository.Get(accountId),
            });
        }

        [HttpGet("{accountId}/transaction/tag/rules/{id}")]
        public async Task<ActionResult<TransactionTagRule>> GetTagRule(Guid accountId, int id)
        {
            return new ActionResult<TransactionTagRule>(await TransactionTagRuleRepository.Get(accountId, id));
        }

        [HttpPost("{accountId}/transaction/tag/rules")]
        public async Task<ActionResult<TransactionTagRule>> PutTagRule(Guid accountId, [FromBody]TransactionTagRuleModel rule)
        {
            var newRule = await TransactionTagRuleRepository.Create(accountId, rule.Contains, rule.Tags);

            return Created($"api/accounts/{accountId}/transaction/tag/rule/{newRule.Id}", newRule);
        }

        [HttpDelete("{accountId}/transaction/tag/rules/{id}")]
        public async Task<ActionResult> DeleteTagRule(Guid accountId, int id)
        {
            await TransactionTagRuleRepository.Delete(accountId, id);

            return NoContent();
        }

        [HttpPut("{accountId}/transaction/tag/rules/{id}/tag/{tagId}")]
        public async Task<ActionResult<TransactionTagRule>> PutTagRuleTag(Guid accountId, int id, int tagId)
        {
            return Created($"api/accounts/{accountId}/transaction/tag/rule/{id}/{tagId}",
                    await TransactionTagRuleRepository.AddTransactionTag(accountId, id, tagId));
        }

        [HttpDelete("{accountId}/transaction/tag/rules/{id}/tag/{tagId}")]
        public async Task<ActionResult> DeleteTagRuleTag(Guid accountId, int id, int tagId)
        {
            await TransactionTagRuleRepository.RemoveTransactionTag(accountId, id, tagId);

            return NoContent();
        }
    }
}