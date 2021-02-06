using System;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private IAccountRepository AccountRepository { get; }
        private ITransactionRepository TransactionRepository { get; }

        public AccountsController(IAccountRepository accountRepository, ITransactionRepository transactionRepository)
        {
            AccountRepository = accountRepository;
            TransactionRepository = transactionRepository;
        }

        [HttpGet]
        public async Task<ActionResult<AccountsModel>> Get()
        {
            return new ActionResult<AccountsModel>(new AccountsModel
            {
                Accounts = await AccountRepository.GetAccounts(),
                Position = await AccountRepository.GetPosition(),
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> Get(Guid id)
        {
            return new ActionResult<Account>(await AccountRepository.GetAccount(id));
        }

        [HttpGet("{accountId}/transactions/{pageSize?}/{pageNumber?}")]
        public async Task<ActionResult<TransactionsModel>> Get(Guid accountId, int? pageSize = 50, int? pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string filter = null, [FromQuery] string sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
        {
            if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

            return new ActionResult<TransactionsModel>(new TransactionsModel
            {
                Transactions = await TransactionRepository.GetTransactions(accountId, filter, start, end, pageSize.Value, pageNumber.Value, sortField, sortDirection),
                PageNumber = pageNumber,
                Total = await TransactionRepository.GetTotalTransactions(accountId, filter, start, end),
            });
        }

        [HttpGet("{accountId}/transactions/untagged/{pageSize?}/{pageNumber?}")]
        public async Task<ActionResult<TransactionsModel>> Getuntagged(Guid accountId, int? pageSize = 50, int? pageNumber = 1, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null, [FromQuery] string filter = null, [FromQuery]string sortField = null, [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
        {
            if (start != null && end != null && end < start) return BadRequest($"{nameof(start)} is less than {nameof(end)}");

            return new ActionResult<TransactionsModel>(new TransactionsModel
            {
                Transactions = await TransactionRepository.GetUntaggedTransactions(accountId, filter, start, end, pageSize.Value, pageNumber.Value, sortField, sortDirection),
                PageNumber = pageNumber,
                Total = await TransactionRepository.GetTotalUntaggedTransactions(accountId, filter, start, end),
            });
        }

        [HttpPost]
        public async Task<ActionResult<Account>> Create(NewAccountModel model)
        {
            Account newAccount = model.ImportAccount != null ?
                await AccountRepository.CreateImportAccount((Account)model.Account, model.ImportAccount.ImporterTypeId) :
                await AccountRepository.Create((Account)model.Account);

            return CreatedAtAction("Get", new { id = newAccount.Id }, newAccount);
        }

        [HttpPatch("{accountId}")]
        public async Task<ActionResult<Account>> UpdateBalance(Guid accountId, UpdateBalanceModel model)
        {
            if (model == null || (model.CurrentBalance == null && model.AvailableBalance == null)) return BadRequest(ModelState);

            return Ok(await AccountRepository.SetBalance(accountId, model.CurrentBalance, model.AvailableBalance));
        }
    }
}