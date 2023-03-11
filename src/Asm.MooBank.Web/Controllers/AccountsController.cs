namespace Asm.MooBank.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, IAccountRepository accountRepository, ITransactionRepository transactionRepository, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    [HttpGet]
    public Task<IEnumerable<Account>> Get(CancellationToken token = default) => _accountRepository.GetAccounts(token);

    [HttpGet("position")]
    public Task<AccountsList> GetFormatted(CancellationToken token = default) => _accountService.GetFormattedAccounts(token);

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
    public async Task<ActionResult<TransactionsModel>> GetUntagged(Guid accountId, int? pageSize = 50, int? pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string filter = null, [FromQuery] string sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
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
