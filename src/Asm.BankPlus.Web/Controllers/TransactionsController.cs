using Asm.BankPlus.DataAccess;
using Asm.BankPlus.Models;
using Asm.BankPlus.Web.Models;
using Asm.BankPlus.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Asm.BankPlus.Web.Controllers
{
    public class TransactionsController : DataAccessController
    {
        [HttpGet]
        public ActionResult Transfer(Guid? id)
        {
            TransferCreateReferenceData();
            TransferModel model = new TransferModel();
            
            if (id.HasValue) model.SourceAccountId = id.Value;

            return View(model);
        }

        [HttpPost]
        public ActionResult Transfer(TransferModel model)
        {
            if (model.SourceAccountId == model.DestinationAccountId)
            {
                ModelState.AddModelError("DestinationAccountId", "The from and to accounts must be different");
            }

            VirtualAccount sourceAccount = BankPlusDb.VirtualAccounts.Where(va => va.VirtualAccountId == model.SourceAccountId).Single();

            if (sourceAccount.Balance < model.Amount)
            {
                ModelState.AddModelError("Amount", "Amount is greater than the balance of the account");
            }

            if (ModelState.IsValid)
            {
                Guid groupId = Guid.NewGuid();

                Transaction source = new Transaction
                {
                    Amount = model.Amount,
                    TransactionType = TransactionType.Debit,
                    TransactionGroupId = groupId,
                    VirtualAccountId = model.SourceAccountId,
                    Description = model.Description,
                };

                Transaction destination = new Transaction
                {
                    Amount = model.Amount,
                    TransactionType = TransactionType.Credit,
                    TransactionGroupId = groupId,
                    VirtualAccountId = model.DestinationAccountId,
                    Description = model.Description,
                };

                BankPlusDb.Transactions.Add(source);
                BankPlusDb.Transactions.Add(destination);

                if (model.RecurringTransaction)
                {
                    RecurringTransaction rc = new RecurringTransaction
                    {
                        Amount = model.Amount,
                        Description = model.Description,
                        DestinationVirtualAccountId = model.DestinationAccountId,
                        LastRun = DateTime.Now,
                        Schedule = model.Schedule,
                        SourceVirtualAccountId = model.SourceAccountId,
                    };

                    BankPlusDb.RecurringTransactions.Add(rc);
                }

                BankPlusDb.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            TransferCreateReferenceData();
            return View(model);
        }

        public ActionResult History(Guid id, DateTime? start, DateTime? end, int? pageSize, int? pageNumber)
        {
            int ps = pageSize ?? 50;
            int pn = pageNumber ?? 1;
            DateTime startTime = start ?? DateTime.MinValue;
            DateTime endTime = end ?? DateTime.MaxValue;

            HistoryModel model = new HistoryModel
            {
                Page = pn,
                PageSize = ps,
                StartDate = startTime,
                EndDate = endTime,
                Account = BankPlusDb.VirtualAccounts.Where(va => va.VirtualAccountId == id).Select(va => va.Name).Single()
            };

            model.TotalRecords = BankPlusDb.Transactions.Where(t => t.VirtualAccountId == id && t.TransactionTime >= startTime && t.TransactionTime <= endTime).OrderByDescending(t => t.TransactionTime).Count();

            model.Transactions =  BankPlusDb.Transactions.Where(t => t.VirtualAccountId == id && t.TransactionTime >= startTime && t.TransactionTime <= endTime).OrderByDescending(t => t.TransactionTime).Skip((pn - 1) * ps).Take(ps).Select(t => new HistoryTransaction {
                Account = t.TransactionType == TransactionType.BalanceAdjustment ? t.Description : BankPlusDb.Transactions.Where(t2 => t2.TransactionGroupId == t.TransactionGroupId && t2.VirtualAccountId != t.VirtualAccountId).Select(t2 => t2.VirtualAccount.Name).FirstOrDefault(),
                Amount = t.Amount,
                Date = t.TransactionTime,
                TransactionType = t.TransactionType,
                Description = t.Description,
            }).ToList();


            return View(model);
        }

        private void TransferCreateReferenceData()
        {
            ViewData["Accounts"] = BankPlusDb.VirtualAccounts.ToList();
            ViewData["Schedules"] = (from Schedule e in Enum.GetValues(typeof(Schedule)) select new KeyValuePair<Schedule,string>(e, e.ToString())).AsEnumerable();
        }
	}
}