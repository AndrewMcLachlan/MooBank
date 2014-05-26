using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Asm.BankPlus.DataAccess;
using Asm.BankPlus.Models;
using Asm.BankPlus.Scrapers;
using Asm.BankPlus.Web.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    [Authorize]
    public class HomeController : DataAccessController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            AccountModel model = new AccountModel();

            model.Accounts = BankPlusDb.Accounts.ToList();

            model.VirtualAccounts = BankPlusDb.VirtualAccounts.ToList();

            return View(model);
        }

        public ActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RefreshBalances()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RefreshBalances(string password)
        {
            MailAccountScraper scraper = new MailAccountScraper();
            var accountBalances = scraper.GetAccountBalances(password);

            foreach (AccountBalance accountBalance in accountBalances)
            {
                var account = BankPlusDb.Accounts.Where(a => a.Name == accountBalance.AccountName).SingleOrDefault();
                if (account != null)
                {
                    account.AccountBalance = accountBalance.CurrentBalance;
                    account.AvailableBalance = accountBalance.AvailableBalance;
                    account.LastUpdated = DateTime.Now;
                }
            }
            BankPlusDb.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}