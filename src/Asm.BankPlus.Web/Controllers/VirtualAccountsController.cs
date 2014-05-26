using Asm.BankPlus.DataAccess;
using Asm.BankPlus.Web.Models;
using Asm.BankPlus.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    public class VirtualAccountsController : DataAccessController
    {
        //
        // GET: /VirtualAccounts/
        public ActionResult Index()
        {
            VirtualAccountsModel model = new VirtualAccountsModel();

            model.VirtualAccounts = BankPlusDb.VirtualAccounts.Where(va => !va.Closed).OrderBy(va => va.Name).ToList();

            return View("Index", model);
        }

        [HttpPost]
        public ActionResult NewAccount()
        {
            VirtualAccount newAccount = new VirtualAccount
            {
                Name = String.Empty,
                Description = String.Empty,
                Balance = 0,
                DefaultAccount = false,
            };

            BankPlusDb.VirtualAccounts.Add(newAccount);

            BankPlusDb.SaveChanges();

            return Index();
        }

        [HttpDelete]
        public ActionResult DeleteAccount(Guid id)
        {
            VirtualAccount virtualAccount = BankPlusDb.VirtualAccounts.Where(va => va.VirtualAccountId == id).SingleOrDefault();

            if (virtualAccount == null)
            {
                return new HttpStatusCodeResult(404, "Not Found");
            }

            if (virtualAccount.Transactions.Count == 0 && virtualAccount.DestinationRecurringTransactions.Count == 0 && virtualAccount.SourceRecurringTransactions.Count == 0)
            {
                BankPlusDb.VirtualAccounts.Remove(virtualAccount);

            }
            else
            {
                virtualAccount.Closed = true;
            }

            BankPlusDb.SaveChanges();

            return Index();
        }

        [HttpPatch]
        public ActionResult UpdateName(Guid id, string name)
        {
            VirtualAccount virtualAccount = BankPlusDb.VirtualAccounts.Where(va => va.VirtualAccountId == id).SingleOrDefault();

            if (virtualAccount == null)
            {
                return new HttpStatusCodeResult(404, "Not Found");
            }

            virtualAccount.Name = name;
            BankPlusDb.SaveChanges();

            return new HttpStatusCodeResult(200, "OK");
        }

        [HttpPatch]
        public ActionResult UpdateDescription(Guid id, string description)
        {
            VirtualAccount virtualAccount = BankPlusDb.VirtualAccounts.Where(va => va.VirtualAccountId == id).SingleOrDefault();

            if (virtualAccount == null)
            {
                return new HttpStatusCodeResult(404, "Not Found");
            }

            virtualAccount.Description = description;
            BankPlusDb.SaveChanges();

            return new HttpStatusCodeResult(200, "OK");
        }

        [HttpPatch]
        public ActionResult SetDefaultAccount(Guid id)
        {
            VirtualAccount virtualAccount = BankPlusDb.VirtualAccounts.Where(va => va.VirtualAccountId == id).SingleOrDefault();
            VirtualAccount currentDefault = BankPlusDb.VirtualAccounts.Where(va => va.DefaultAccount).SingleOrDefault();

            if (virtualAccount == null)
            {
                return new HttpStatusCodeResult(404, "Not Found");
            }

            if (currentDefault != null)
            {
                currentDefault.DefaultAccount = false;
                BankPlusDb.SaveChanges();
            }

            virtualAccount.DefaultAccount = true;
            BankPlusDb.SaveChanges();            

            return new HttpStatusCodeResult(200, "OK");
        }
    }
}