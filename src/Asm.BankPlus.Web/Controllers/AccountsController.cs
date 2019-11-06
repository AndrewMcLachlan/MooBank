﻿using System;
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
        public async Task<ActionResult<TransactionsModel>> Get(Guid accountId, int? pageSize = 50, int? pageNumber = 1)
        {
            return new ActionResult<TransactionsModel>(new TransactionsModel
            {
                Transactions = await TransactionRepository.GetTransactions(accountId, pageSize.Value, pageNumber.Value),
                PageNumber = pageNumber,
                Total = await TransactionRepository.GetTotalTransactions(accountId),
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

    }
}