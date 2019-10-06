﻿using System;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Services;
using Asm.BankPlus.Web.Models;
using Asm.BankPlus.Web.Mvc;
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
        private IAntiforgery Antiforgery { get; }

        public AccountsController(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IAntiforgery antiforgery)
        {
            AccountRepository = accountRepository;
            TransactionRepository = transactionRepository;
            Antiforgery = antiforgery;
        }

        // [ValidateAntiForgeryToken]
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
    }
}