using System;
using System.Threading.Tasks;
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

        public AccountsController(IAccountRepository accountRepository, IAntiforgery antiforgery)
        {
            AccountRepository = accountRepository;
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
    }
}