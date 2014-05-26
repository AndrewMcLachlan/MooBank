using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Asm.BankPlus.DataAccess;
using Asm.BankPlus.Models;
using Asm.BankPlus.Scrapers;
using Asm.BankPlus.Transactions;
using log4net;

namespace Asm.BankPlus.Service
{
    public partial class BankPlusService : ServiceBase
    {
        #region Constants
        private readonly TimeSpan AccountBalanceReadTime = TimeSpan.Parse(ConfigurationManager.AppSettings["AccountBalanceReadTime"]);
        private static readonly object SyncRoot = new object();
        #endregion

        #region Fields
        private DateTime _accountBalanceReadDate = DateTime.MinValue;
        private Timer _pollTimer = new Timer();
        private ILog _log = LogManager.GetLogger(typeof(BankPlusService));
        #endregion

        public BankPlusService()
        {
            _pollTimer.Elapsed += PollTimer_Elapsed;
            _pollTimer.Interval = Int32.Parse(ConfigurationManager.AppSettings["PollingInterval"]);
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log.Info("BankPlus Service");
            _log.Info("Version " + FileVersionInfo.GetVersionInfo(typeof(BankPlusService).Assembly.Location).FileVersion);
            _log.Info("Starting...");
            _pollTimer.Start();
        }

        protected override void OnStop()
        {
            _log.Info("Stopping...");
            _pollTimer.Stop();
        }

        protected override void OnPause()
        {
            _log.Info("Pausing...");
            _pollTimer.Stop();
        }

        protected override void OnContinue()
        {
            _log.Info("Continuing...");
            _pollTimer.Start();
        }

        private void PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (SyncRoot)
            {
                ReadAccountBalances();
                ProcessRecurringTransactions();
            }
        }

        private void ReadAccountBalances()
        {
            if (DateTime.Now.Date > _accountBalanceReadDate.Date && DateTime.Now.TimeOfDay > AccountBalanceReadTime)
            {
                try
                {
                    IAccountScraper accountScraper = new MailAccountScraper();

                    IEnumerable<AccountBalance> accountBalances = accountScraper.GetAccountBalances();

                    using (BankPlusContext db = new BankPlusContext())
                    {
                        foreach (AccountBalance accountBalance in accountBalances)
                        {
                            var account = db.Accounts.Where(a => a.Name == accountBalance.AccountName).SingleOrDefault();
                            if (account != null)
                            {
                                account.AccountBalance = accountBalance.CurrentBalance;
                                account.AvailableBalance = accountBalance.AvailableBalance;
                                account.LastUpdated = DateTime.Now;
                            }
                        }

                        db.SaveChanges();
                    }

                    _accountBalanceReadDate = DateTime.Now.Date;
                }
                catch (Exception ex)
                {
                    _log.Error("Error updating account balances: ", ex);
                    throw;
                }
            }
        }

        private void ProcessRecurringTransactions()
        {
            try
            {
                RecurringTransactions.Process();
            }
            catch (Exception ex)
            {
                _log.Error("Error processing recurring transactions: ", ex);
                throw;
            }
        }
    }
}
