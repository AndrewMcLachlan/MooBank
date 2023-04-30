using System.Net;
using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Commands.AccountGroup;
using Asm.MooBank.Models.Commands.TransactionTagRules;

namespace Asm.MooBank.Web.Controllers
{
    [Route("api/accounts/{accountId}/transaction/tag/rules")]
    [ApiController]
    [Authorize]
    public class AccountTransactionTagRulesController : CommandQueryController
    {
        private IAccountService AccountService { get; }
        private ITransactionTagRuleService TransactionTagRuleService { get; }

        public AccountTransactionTagRulesController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ITransactionTagRuleService transactionTagRuleService, IAccountService accountService) : base(queryDispatcher, commandDispatcher)
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
        public async Task<ActionResult<TransactionTagRule>> Post(Guid accountId, [FromBody]TransactionTagRuleModel rule, CancellationToken cancellationToken = default)
        {
            var newRule = await TransactionTagRuleService.Create(accountId, rule.Contains, rule.Description, rule.Tags, cancellationToken);

            return Created($"api/accounts/{accountId}/transaction/tag/rule/{newRule.Id}", newRule);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TransactionTagRule>> Update(Guid accountId, int id, [FromBody]TransactionTagRule rule, CancellationToken cancellationToken = default)
        {
            if (rule.Id != id) return BadRequest();

            var updateRule = new UpdateRule(accountId, id, rule);

            return Ok(await CommandDispatcher.Dispatch(updateRule, cancellationToken));
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