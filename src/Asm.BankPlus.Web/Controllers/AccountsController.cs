﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private BankPlusContext DataContext { get; }

        public AccountsController(BankPlusContext dataContext)
        {
            DataContext = dataContext;
        }

        public async Task<ActionResult<AccountsModel>> Get()
        {
            return new ActionResult<AccountsModel>(new AccountsModel
            {
                Accounts = await DataContext.Account.ToListAsync(),
                VirtualAccounts = await DataContext.VirtualAccount.ToListAsync(),
            }); ;
        }
    }
}