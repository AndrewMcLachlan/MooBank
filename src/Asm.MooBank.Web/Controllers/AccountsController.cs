﻿namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;

    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ITransactionService transactionService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpGet]
    public Task<IEnumerable<InstitutionAccount>> Get(CancellationToken token = default) => _accountService.GetAccounts(token);

    [HttpGet("position")]
    public Task<AccountsList> GetFormatted(CancellationToken token = default) => _accountService.GetFormattedAccounts(token);

    [HttpGet("{id}")]
    public async Task<ActionResult<InstitutionAccount>> Get(Guid id)
    {
        return new ActionResult<InstitutionAccount>(await _accountService.GetAccount(id));
    }

    [HttpGet("{accountId}/transactions/{pageSize?}/{pageNumber?}")]
    public async Task<ActionResult<TransactionsModel>> Get(Guid accountId, int? pageSize = 50, int? pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string filter = null, [FromQuery] string sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
    {
        if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

        return new ActionResult<TransactionsModel>(new TransactionsModel
        {
            Transactions = await _transactionService.GetTransactions(accountId, filter, start, end, pageSize.Value, pageNumber.Value, sortField, sortDirection),
            PageNumber = pageNumber,
            Total = await _transactionService.GetTotalTransactions(accountId, filter, start, end),
        });
    }

    [HttpGet("{accountId}/transactions/untagged/{pageSize?}/{pageNumber?}")]
    public async Task<ActionResult<TransactionsModel>> GetUntagged(Guid accountId, int? pageSize = 50, int? pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string filter = null, [FromQuery] string sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
    {
        if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

        return new ActionResult<TransactionsModel>(new TransactionsModel
        {
            Transactions = await _transactionService.GetUntaggedTransactions(accountId, filter, start, end, pageSize.Value, pageNumber.Value, sortField, sortDirection),
            PageNumber = pageNumber,
            Total = await _transactionService.GetTotalUntaggedTransactions(accountId, filter, start, end),
        });
    }

    [HttpPost]
    public async Task<ActionResult<InstitutionAccount>> Create(NewAccountModel model)
    {
        var newAccount = await _accountService.Create(model.Account);

        return CreatedAtAction("Get", new { id = newAccount.Id }, newAccount);
    }

    [HttpPatch("{accountId}")]
    public async Task<ActionResult<InstitutionAccount>> UpdateAccount(Guid accountId, InstitutionAccount model)
    {
        if (model == null || model.Id != accountId) return BadRequest(ModelState);

        return Ok(await _accountService.Update(model));
    }

    [HttpPatch("{accountId}/balance")]
    public async Task<ActionResult<InstitutionAccount>> UpdateBalance(Guid accountId, UpdateBalanceModel model)
    {
        if (model == null || (model.CurrentBalance == null)) return BadRequest(ModelState);

        return Ok(await _accountService.SetBalance(accountId, model.CurrentBalance.Value));
    }
}
