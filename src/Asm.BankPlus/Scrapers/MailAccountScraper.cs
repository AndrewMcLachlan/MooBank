using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using AE.Net.Mail;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Scrapers
{
    public class MailAccountScraper : IAccountScraper
    {
        #region Constants
        private readonly Regex accountNamePattern = new Regex(@"^Your current account balance for the account (.*?) is:", RegexOptions.Multiline);
        private readonly Regex currentBalancePattern = new Regex(@"^Current Balance\*?: *(-?)\$(.*)$", RegexOptions.Multiline);
        private readonly Regex availableBalancePattern = new Regex(@"^Available (Credit|Balance)\*?: *(-?)\$(.*)$", RegexOptions.Multiline);
        #endregion

        #region Public Methods
        public IEnumerable<AccountBalance> GetAccountBalances()
        {
            return GetAccountBalances(ConfigurationManager.AppSettings["MailPassword"]);
        }
        public IEnumerable<AccountBalance> GetAccountBalances(string mailPassword)
        {
            List<AccountBalance> accounts = new List<AccountBalance>();

            string mailHost = ConfigurationManager.AppSettings["MailHost"];
            int mailPort = Int32.Parse(ConfigurationManager.AppSettings["MailPort"]);
            string mailUser = ConfigurationManager.AppSettings["MailUser"];
            string mailbox = ConfigurationManager.AppSettings["Mailbox"];

            IEnumerable<MailMessage> messages;

            using (ImapClient client = new ImapClient(mailHost, mailUser, mailPassword, ImapClient.AuthMethods.Login, mailPort, true, false))
            {
                var m = client.SelectMailbox(mailbox);
                int messageCount = client.GetMessageCount();

                messages = client.GetMessages(messageCount-1, messageCount-6, false);
            }

            foreach (var message in messages)
            {
                AccountBalance ab = ProcessMessage(message);

                AccountBalance current = accounts.Where(a => a.AccountName == ab.AccountName).SingleOrDefault();

                if (current == null)
                {
                    accounts.Add(ab);
                }
                else if (current != null && current.BalanceDate < ab.BalanceDate)
                {
                    accounts[accounts.IndexOf(current)] = ab;
                }
            }

            return accounts;
        }
        #endregion

        #region Private Methods
        private AccountBalance ProcessMessage(MailMessage message)
        {
            AccountBalance accountBalance = new AccountBalance() { BalanceDate = message.Date };

            string body = message.Body;

            Match anMatch = accountNamePattern.Match(body);

            accountBalance.AccountName = anMatch.Groups[1].Value;

            Match cbMatch = currentBalancePattern.Match(body);
            string cbSign = cbMatch.Groups[1].Value;
            string cb = cbSign + cbMatch.Groups[2].Value.Trim();

            accountBalance.CurrentBalance = Decimal.Parse(cb.Replace(",", String.Empty));


            Match abMatch = availableBalancePattern.Match(body);
            string abSign = abMatch.Groups[2].Value;
            string ab = abSign + abMatch.Groups[3].Value.Trim();
            accountBalance.AvailableBalance = Decimal.Parse(ab.Replace(",", String.Empty));

            return accountBalance;
        }
        #endregion
    }
}
