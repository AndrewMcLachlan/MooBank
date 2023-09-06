using System.Net;
using Asm.Cqrs.Commands;
using Asm.Cqrs.Queries;
using Asm.MooBank.Commands.Rules;

namespace Asm.MooBank.Web.Controllers
{
    [Route("api/accounts/{accountId}/transaction/tag/rules")]
    [Route("api/accounts/{accountId}/rules")]
    [ApiController]
    [Authorize]
    public class AccountRulesController : CommandQueryController
    {
        private readonly IAccountService _accountService;
        private readonly IRuleService _ruleService;

        public AccountRulesController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IRuleService transactionTagRuleService, IAccountService accountService) : base(queryDispatcher, commandDispatcher)
        {
            _ruleService = transactionTagRuleService;
            _accountService = accountService;
        }

        [HttpGet]
        [ActionName("Get All Rules")]
        public async Task<ActionResult<TransactionTagRulesModel>> Get(Guid accountId)
        {
            return new ActionResult<TransactionTagRulesModel>(new TransactionTagRulesModel
            {
                Rules = await _ruleService.Get(accountId),
            });
        }

        [HttpGet("{id}")]
        [ActionName("Get")]
        public async Task<ActionResult<Rule>> Get(Guid accountId, int id)
        {
            return new ActionResult<Rule>(await _ruleService.Get(accountId, id));
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<ActionResult<Rule>> Post(Guid accountId, [FromBody] TransactionTagRuleModel rule, CancellationToken cancellationToken = default)
        {
            var newRule = await _ruleService.Create(accountId, rule.Contains, rule.Description, rule.Tags, cancellationToken);

            return CreatedAtAction(nameof(Get), new { accountId, id = newRule.Id }, newRule);
        }

        [HttpPatch("{id}")]
        [ActionName("Update")]
        public async Task<ActionResult<Rule>> Update(Guid accountId, int id, [FromBody] Rule rule, CancellationToken cancellationToken = default)
        {
            if (rule.Id != id) return BadRequest();

            var updateRule = new UpdateRule(accountId, id, rule);

            return Ok(await CommandDispatcher.Dispatch(updateRule, cancellationToken));
        }

        [HttpDelete("{id}")]
        [ActionName("Delete Rule")]
        public async Task<ActionResult> Delete(Guid accountId, int id)
        {
            await _ruleService.Delete(accountId, id);

            return NoContent();
        }

        [HttpPut("{id}/tag/{tagId}")]
        [ActionName("Add Tag")]
        public async Task<ActionResult<Rule>> PutTag(Guid accountId, int id, int tagId) =>
             CreatedAtAction(nameof(TagsController.Get), ControllerName<TagsController>(), new { id = tagId }, await _ruleService.AddTransactionTag(accountId, id, tagId));

        [HttpDelete("{id}/tag/{tagId}")]
        [ActionName("Remove Tag")]
        public async Task<ActionResult> DeleteTag(Guid accountId, int id, int tagId)
        {
            await _ruleService.RemoveTransactionTag(accountId, id, tagId);

            return NoContent();
        }

        [HttpPost("run")]
        [ActionName("Run Rules")]
        public ActionResult Run(Guid accountId)
        {
            _accountService.RunTransactionRules(accountId);

            return StatusCode((int)HttpStatusCode.Accepted);
        }

        public string ControllerName<T>()
        {
            return nameof(T).Replace("Controller", String.Empty);
        }
    }
}