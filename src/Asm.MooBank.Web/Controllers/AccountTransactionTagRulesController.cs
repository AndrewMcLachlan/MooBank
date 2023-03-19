﻿using System.Net;

namespace Asm.MooBank.Web.Controllers
{
    [Route("api/accounts/{accountId}/transaction/tag/rules")]
    [ApiController]
    [Authorize]
    public class AccountTransactionTagRulesController : ControllerBase
    {
        private IAccountService AccountService { get; }
        private ITransactionTagRuleService TransactionTagRuleService { get; }

        public AccountTransactionTagRulesController(ITransactionTagRuleService transactionTagRuleService, IAccountService accountService)
        {
            TransactionTagRuleService = transactionTagRuleService;
            AccountService = accountService;
        }

        [HttpGet]
        public async Task<ActionResult<TransactionTagRulesModel>> Get(Guid accountId)
        {
            return new ActionResult<TransactionTagRulesModel>(new TransactionTagRulesModel
            {
                Rules = await TransactionTagRuleService.Get(accountId),
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionTagRule>> Get(Guid accountId, int id)
        {
            return new ActionResult<TransactionTagRule>(await TransactionTagRuleService.Get(accountId, id));
        }

        [HttpPost]
        public async Task<ActionResult<TransactionTagRule>> Post(Guid accountId, [FromBody]TransactionTagRuleModel rule)
        {
            var newRule = await TransactionTagRuleService.Create(accountId, rule.Contains, rule.Tags);

            return Created($"api/accounts/{accountId}/transaction/tag/rule/{newRule.Id}", newRule);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid accountId, int id)
        {
            await TransactionTagRuleService.Delete(accountId, id);

            return NoContent();
        }

        [HttpPut("{id}/tag/{tagId}")]
        public async Task<ActionResult<TransactionTagRule>> PutTag(Guid accountId, int id, int tagId)
        {
            return Created($"api/accounts/{accountId}/transaction/tag/rule/{id}/{tagId}",
                    await TransactionTagRuleService.AddTransactionTag(accountId, id, tagId));
        }

        [HttpDelete("{id}/tag/{tagId}")]
        public async Task<ActionResult> DeleteTag(Guid accountId, int id, int tagId)
        {
            await TransactionTagRuleService.RemoveTransactionTag(accountId, id, tagId);

            return NoContent();
        }

        [HttpPost("run")]
        public ActionResult Run(Guid accountId)
        {
            AccountService.RunTransactionRules(accountId);

            return StatusCode((int)HttpStatusCode.Accepted);
        }
    }
}