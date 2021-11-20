using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountRepository accountRepository, ITransactionRepository transactionRepository, ILogger<AccountsController> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AccountsModel>> Get()
    {
        return new ActionResult<AccountsModel>(new AccountsModel
        {
            Accounts = await _accountRepository.GetAccounts(),
            Position = await _accountRepository.GetPosition(),
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> Get(Guid id)
    {
        return new ActionResult<Account>(await _accountRepository.GetAccount(id));
    }

    [HttpGet("{accountId}/transactions/{pageSize?}/{pageNumber?}")]
    public async Task<ActionResult<TransactionsModel>> Get(Guid accountId, int? pageSize = 50, int? pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string filter = null, [FromQuery] string sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
    {
        if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

        return new ActionResult<TransactionsModel>(new TransactionsModel
        {
            Transactions = await _transactionRepository.GetTransactions(accountId, filter, start, end, pageSize.Value, pageNumber.Value, sortField, sortDirection),
            PageNumber = pageNumber,
            Total = await _transactionRepository.GetTotalTransactions(accountId, filter, start, end),
        });
    }

    [HttpGet("{accountId}/transactions/untagged/{pageSize?}/{pageNumber?}")]
    public async Task<ActionResult<TransactionsModel>> Getuntagged(Guid accountId, int? pageSize = 50, int? pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string filter = null, [FromQuery] string sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
    {
        if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

        return new ActionResult<TransactionsModel>(new TransactionsModel
        {
            Transactions = await _transactionRepository.GetUntaggedTransactions(accountId, filter, start, end, pageSize.Value, pageNumber.Value, sortField, sortDirection),
            PageNumber = pageNumber,
            Total = await _transactionRepository.GetTotalUntaggedTransactions(accountId, filter, start, end),
        });
    }

    [HttpPost]
    public async Task<ActionResult<Account>> Create(NewAccountModel model)
    {
        Account newAccount = model.ImportAccount != null ?
            await _accountRepository.CreateImportAccount((Account)model.Account, model.ImportAccount.ImporterTypeId) :
            await _accountRepository.Create((Account)model.Account);

        return CreatedAtAction("Get", new { id = newAccount.Id }, newAccount);
    }

    [HttpPatch("{accountId}")]
    public async Task<ActionResult<Account>> UpdateAccount(Guid accountId, Account model)
    {
        if (model == null || model.Id != accountId) return BadRequest(ModelState);

        return Ok(await _accountRepository.Update(model));
    }

    [HttpPatch("{accountId}/balance")]
    public async Task<ActionResult<Account>> UpdateBalance(Guid accountId, UpdateBalanceModel model)
    {
        if (model == null || (model.CurrentBalance == null && model.AvailableBalance == null)) return BadRequest(ModelState);

        return Ok(await _accountRepository.SetBalance(accountId, model.CurrentBalance, model.AvailableBalance));
    }
}
