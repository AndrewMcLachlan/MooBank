using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Transaction = Asm.BankPlus.Models.Transaction;

namespace Asm.BankPlus.Transactions
{
    public static class TransactionProcessor
    {
        public static async Task Transfer(decimal amount, Guid sourceAccountId, Guid destinationAccountId, bool isRecurring, string description = null)
        {
            TransactionType sourceType = isRecurring ? TransactionType.RecurringDebit : TransactionType.Debit;
            TransactionType destinationType = isRecurring ? TransactionType.RecurringCredit : TransactionType.Credit;

            using (BankPlusContext db = new BankPlusContext())
            {
                Guid groupId = Guid.NewGuid();

                var source = new Data.Entities.Transaction
                {
                    Amount = amount,
                    TransactionType = sourceType,
                    //TransactionReference = groupId,
                    AccountId = sourceAccountId,
                    Description = description,
                };

                var destination = new Data.Entities.Transaction
                {
                    Amount = amount,
                    TransactionType = destinationType,
                    //TransactionReference = groupId,
                    AccountId = destinationAccountId,
                    Description = description,
                };

                db.Transactions.Add(source);
                db.Transactions.Add(destination);

                await db.SaveChangesAsync();
            }
        }

        public static void Import(IEnumerable<Transaction> transactions)
        {
            using (var db = new BankPlusContext())
            {

            }
        }
    }
}
